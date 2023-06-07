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
	private readonly IMediator _mediator;
	private readonly IRequestContext _requestContext;
	protected readonly IUserRepository UserRepository;
	protected readonly ILockRepository LockRepository;
	protected readonly ILockGroupRepository LockGroupRepository;
	
	public ValidateLockAccessCommandHandler(IUserRepository userRepository, ILockRepository lockRepository,
		ILockGroupRepository lockGroupRepository, IMediator mediator, IRequestContext requestContext)
	{
		UserRepository = userRepository;
		LockRepository = lockRepository;
		LockGroupRepository = lockGroupRepository;
		_mediator = mediator;
		_requestContext = requestContext;
	}

	protected async Task<bool> DoesUserHaveAccessForLockAsync(Guid lockId, User userObj,
		CancellationToken cancellationToken)
	{
		var lockGroups =
			await LockGroupRepository.GetByLockIdAsync(lockId, _requestContext.OrganizationId, cancellationToken);

		if (lockGroups.Any(x => userObj.PermittedLockGroups.Any(y => y.Id == x.Id)))
		{
			return true;
		}

		return false;
	}

	protected async Task<Lock> GetLockAsync(Guid lockId, CancellationToken cancellationToken)
	{
		var lockObj = await LockRepository.GetByIdAsync(lockId, _requestContext.OrganizationId, cancellationToken);
		if (lockObj == null)
		{
			throw new AppBusinessException($"Lock [Id={lockId}] does not exist");
		}

		return lockObj;
	}

	protected async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
	{
		var user = await UserRepository.GetByIdAsync(userId, _requestContext.OrganizationId, cancellationToken);
		if (user == null)
		{
			throw new AppBusinessException($"User [Id={userId}] does not exist");
		}

		return user;
	}

	protected async Task PublishLockAccessEvent(Lock lockObj, User? user, AccessState accessState,
		AccessDeniedReason? accessDeniedReason, CancellationToken cancellationToken)
	{
		await _mediator.Publish(new LockAccessedEvent(lockObj, user, accessState, accessDeniedReason),
			cancellationToken);
	}

	protected LockAccessResponse AccessDenied(string message)
	{
		return new LockAccessResponse { AccessAllowed = false, Message = message };
	}

	protected LockAccessResponse AccessAllowed() => new LockAccessResponse { AccessAllowed = true };
}