using MediatR;
using SLCP.Business.Exception;
using SLCP.Business.Notification;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessCommandHandler
{
	protected readonly IMediator Mediator;
	protected readonly IRequestContext RequestContext;
	protected readonly IUserRepository UserRepository;
	protected readonly ILockRepository LockRepository;
	protected readonly ILockGroupRepository LockGroupRepository;
	
	public ValidateLockAccessCommandHandler(IUserRepository userRepository, ILockRepository lockRepository,
		ILockGroupRepository lockGroupRepository, IMediator mediator, IRequestContext requestContext)
	{
		UserRepository = userRepository;
		LockRepository = lockRepository;
		LockGroupRepository = lockGroupRepository;
		RequestContext = requestContext;
		Mediator = mediator;
	}

	protected async Task<LockAccessResponse> DoesUserHaveAccessForLockAsync(Lock lockObj, User userObj,
		CancellationToken cancellationToken)
	{
		var lockGroups =
			await LockGroupRepository.GetByLockIdAsync(lockObj.Id, RequestContext.OrganizationId, cancellationToken);

		if (lockGroups.Any(x => userObj.PermittedLockGroups.Any(y => y.Id == x.Id)))
		{
			await PublishOpenLockEvent(lockObj, cancellationToken);
			await PublishLockAccessEvent(lockObj, userObj, AccessState.Allowed, null, cancellationToken);
			return AccessAllowed();
		}

		await PublishLockAccessEvent(lockObj, userObj, AccessState.Denied,
			AccessDeniedReason.DoesNotHaveAccessToLock, cancellationToken);

		return AccessDenied("User does not have access to the lock");
	}

	protected async Task<Lock> GetLockAsync(Guid lockId, CancellationToken cancellationToken)
	{
		var lockObj = await LockRepository.GetByIdAsync(lockId, RequestContext.OrganizationId, cancellationToken);
		if (lockObj == null)
		{
			throw new AppBusinessException($"Lock [Id={lockId}] does not exist");
		}

		return lockObj;
	}

	protected async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
	{
		var user = await UserRepository.GetByIdAsync(userId, RequestContext.OrganizationId, cancellationToken);
		if (user == null)
		{
			throw new AppBusinessException($"User [Id={userId}] does not exist");
		}

		return user;
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

	protected LockAccessResponse AccessDenied(string message)
	{
		return new LockAccessResponse { AccessAllowed = false, Message = message };
	}

	protected LockAccessResponse AccessAllowed() => new LockAccessResponse { AccessAllowed = true };
}