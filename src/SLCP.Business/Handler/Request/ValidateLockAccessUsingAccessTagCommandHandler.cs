using MediatR;
using SLCP.Business.Exception;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessTagCommandHandler : ValidateLockAccessCommandHandler,
	IRequestHandler<ValidateLockAccessUsingAccessTagCommand, LockAccessResponse>
{
	private readonly IAccessTagRepository _accessTagRepository;

	public ValidateLockAccessUsingAccessTagCommandHandler(IUserRepository userRepository,
		ILockRepository lockRepository, ILockGroupRepository lockGroupRepository, IAccessTagRepository accessTagRepository, IMediator mediator,
		IRequestContext requestContext) : base(userRepository, lockRepository, lockGroupRepository, mediator, requestContext)
	{
		_accessTagRepository = accessTagRepository;
	}

	public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessTagCommand request,
		CancellationToken cancellationToken)
	{
		var lockObj = await GetLockAsync(request.LockId, cancellationToken);

		var accessTag = await GetAccessTagAsync(request.AccessTagId, lockObj.OrganizationId,
			cancellationToken);

		var user = await GetUserAsync(accessTag.User.Id, cancellationToken);

		if (accessTag.IsBlocked)
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Denied,
				AccessDeniedReason.AccessTagBlocked, cancellationToken);
			return AccessDenied($"AccessTag [Id={request.AccessTagId}] is blocked");
		}

		if (await DoesUserHaveAccessForLockAsync(lockObj.Id, user, cancellationToken))
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Allowed, null, cancellationToken);
			return AccessAllowed();
		}
		else
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Denied,
				AccessDeniedReason.DoesNotHaveAccessToLock, cancellationToken);
			return AccessDenied("User does not have access to the lock");
		}
	}

	private async Task<AccessTag> GetAccessTagAsync(Guid accessTagId, Guid orgId, CancellationToken cancellationToken)
	{
		var accessTag = await _accessTagRepository.GetByIdAsync(accessTagId, orgId, cancellationToken);

		if (accessTag == null)
		{
			throw new AppBusinessException($"AccessTag [Id={accessTagId}] does not exist");
		}

		return accessTag;
	}
}