﻿using MediatR;
using SLCP.Business.Exception;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessTagCommandHandler : ValidateLockAccessCommandHandler, IRequestHandler<ValidateLockAccessUsingAccessTagCommand, LockAccessResponse>
{
    private readonly IAccessTagRepository _accessTagRepository;

    public ValidateLockAccessUsingAccessTagCommandHandler(IUserRepository userRepository,
	    ILockRepository lockRepository,
	    ILockGroupRepository lockGroupRepository, IAccessTagRepository accessTagRepository, IMediator mediator) : base(
	    userRepository, lockRepository, lockGroupRepository, mediator)
    {
	    _accessTagRepository = accessTagRepository;
    }

    public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessTagCommand request, CancellationToken cancellationToken)
    {
        var lockObj = await GetLockAsync(request.LockId, cancellationToken);

        var accessTag = await GetAccessTagAsync(request.AccessTagId, lockObj.Organization.Id,
	        cancellationToken);

        var user = await GetUserAsync(accessTag.User.Id, lockObj.Organization.Id, cancellationToken);

		if (accessTag.IsBlocked)
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Denied,
				AccessDeniedReason.AccessTagBlocked, cancellationToken);
			return AccessDenied($"AccessTag [Id={request.AccessTagId}] is blocked");
		}

		if (await DoesUserHaveAccessForLockAsync(lockObj, user, cancellationToken))
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
            throw new DomainException($"AccessTag [Id={accessTagId}] does not exist");
        }

        return accessTag;
    }
}