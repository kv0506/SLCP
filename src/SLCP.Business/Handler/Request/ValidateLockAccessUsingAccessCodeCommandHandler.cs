using CSharpExtensions;
using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessCodeCommandHandler : ValidateLockAccessCommandHandler,
	IRequestHandler<ValidateLockAccessUsingAccessCodeCommand, LockAccessResponse>
{
	public ValidateLockAccessUsingAccessCodeCommandHandler(IUserRepository userRepository,
		ILockRepository lockRepository, ILockGroupRepository lockGroupRepository, IMediator mediator, IRequestContext requestContext) : 
		base(userRepository, lockRepository, lockGroupRepository, mediator, requestContext)
	{
	}

	public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessCodeCommand request,
		CancellationToken cancellationToken)
	{
		var lockObj = await GetLockAsync(request.LockId, cancellationToken);

		var user = await GetUserAsync(request.UserId, cancellationToken);

		if (request.UserLockAccessCode.IsNotEquals(user.LockAccessCode))
		{
			await PublishLockAccessEvent(lockObj, user, AccessState.Denied,
				AccessDeniedReason.InvalidUserLockAccessCode, cancellationToken);
			return AccessDenied("Lock access code is invalid");
		}

		return await DoesUserHaveAccessForLockAsync(lockObj, user, cancellationToken);
	}
}