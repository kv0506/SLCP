using MediatR;
using SLCP.Business.Notification;
using SLCP.Business.Request;
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
	protected readonly IUserAccessGroupRepository UserAccessGroupRepository;

	public ValidateLockAccessCommandHandler(IUserAccessGroupRepository userAccessGroupRepository, IMediator mediator,
		IRequestContext requestContext)
	{
		UserAccessGroupRepository = userAccessGroupRepository;
		RequestContext = requestContext;
		Mediator = mediator;
	}

	protected async Task<LockAccessResponse> DoesUserHaveAccessForLockAsync(ValidateLockAccessCommand command, User userObj,
		CancellationToken cancellationToken)
	{
		var userAccessGroups = await UserAccessGroupRepository.GetByLockIdAsync(command.LockId, command.LocationId, cancellationToken);
		var lockObj = userAccessGroups.SelectMany(x => x.Locks).FirstOrDefault(x => x.Id == command.LockId);
		if (lockObj == null)
		{
			return AccessDenied(ErrorCode.NotFound, "Lock does not exit");
		}

		if (userAccessGroups.SelectMany(x => x.Users).Any(x => x.Id == userObj.Id))
		{
			await PublishOpenLockEvent(lockObj, cancellationToken);
			await PublishLockAccessEvent(lockObj, userObj, AccessState.Allowed, null, cancellationToken);
			return AccessAllowed();
		}

		await PublishLockAccessEvent(lockObj, userObj, AccessState.Denied,
			AccessDeniedReason.DoesNotHaveAccessToLock, cancellationToken);

		return AccessDenied(ErrorCode.DoesNotHaveAccessToLock, "User does not have access to the lock");
	}

	protected async Task PublishLockAccessEvent(Lock lockObj, User user, AccessState accessState,
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