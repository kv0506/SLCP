using MediatR;
using SLCP.Business.Notification;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessCommandHandler
{
	protected readonly IMediator Mediator;
	protected readonly IRequestContext RequestContext;
	protected readonly ILockRepository LockRepository;

	public ValidateLockAccessCommandHandler(ILockRepository lockRepository, IMediator mediator,
		IRequestContext requestContext)
	{
		LockRepository = lockRepository;
		RequestContext = requestContext;
		Mediator = mediator;
	}

	protected async Task<LockAccessResponse> DoesUserHaveAccessForLockAsync(Lock lockObj, User userObj,
		CancellationToken cancellationToken)
	{
		if (userObj.PermittedLockGroups.SelectMany(x => x.Locks).Any(x => x.Id == lockObj.Id))
		{
			await PublishOpenLockEvent(lockObj, cancellationToken);
			await PublishLockAccessEvent(lockObj, userObj, AccessState.Allowed, null, cancellationToken);
			return AccessAllowed();
		}

		await PublishLockAccessEvent(lockObj, userObj, AccessState.Denied,
			AccessDeniedReason.DoesNotHaveAccessToLock, cancellationToken);

		return AccessDenied(ErrorCode.DoesNotHaveAccessToLock, "User does not have access to the lock");
	}

	protected async Task<Lock> GetLockAsync(Guid lockId, CancellationToken cancellationToken)
	{
		var lockObj = await LockRepository.GetByIdAsync(lockId, RequestContext.OrganizationId, cancellationToken);
		if (lockObj == null)
		{
			throw new AppException(ErrorCode.NotFound, $"Lock [Id={lockId}] does not exist");
		}

		return lockObj;
	}

	protected async Task PublishLockAccessEvent(Lock lockObj, User? user, AccessState accessState,
		AccessDeniedReason? accessDeniedReason, CancellationToken cancellationToken)
	{
		await Mediator.Publish(new LockAccessedEvent(lockObj, user, accessState, accessDeniedReason),
			cancellationToken);
	}

	protected async Task PublishOpenLockEvent(Lock lockObj, CancellationToken cancellationToken)
	{
		await Mediator.Publish(new OpenLockEvent(lockObj), cancellationToken);
	}

	protected LockAccessResponse AccessDenied(ErrorCode errorCode, string message)
	{
		throw new AppException(errorCode, message);
	}

	protected LockAccessResponse AccessAllowed() => new LockAccessResponse { AccessAllowed = true };
}