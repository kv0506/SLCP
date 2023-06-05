using CSharpExtensions;
using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessCodeCommandHandler : ValidateLockAccessCommandHandler, IRequestHandler<ValidateLockAccessUsingAccessCodeCommand, LockAccessResponse>
{
	public ValidateLockAccessUsingAccessCodeCommandHandler(IUserRepository userRepository, ILockRepository lockRepository,
		ILockGroupRepository lockGroupRepository, IMediator mediator) : base(userRepository, lockRepository, lockGroupRepository, mediator)
	{
	}

	public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessCodeCommand request, CancellationToken cancellationToken)
	{
		var lockObj = await GetLockAsync(request.LockId, cancellationToken);

		var user = await GetUserAsync(request.UserId, lockObj.Organization.Id, cancellationToken);

		if (request.UserLockAccessCode.IsNotEquals(user.LockAccessCode))
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Denied, AccessDeniedReason.InvalidUserLockAccessCode, cancellationToken);
			return AccessDenied("Lock access code is invalid");
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
}