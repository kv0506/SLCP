using CSharpExtensions;
using MediatR;
using SLCP.Business.Exception;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessCodeCommandHandler : ValidateLockAccessCommandHandler,
	IRequestHandler<ValidateLockAccessUsingAccessCodeCommand, LockAccessResponse>
{
	private readonly IUserRepository _userRepository;

	public ValidateLockAccessUsingAccessCodeCommandHandler(IUserRepository userRepository,
		ILockRepository lockRepository, IMediator mediator, IRequestContext requestContext) : 
		base(lockRepository, mediator, requestContext)
	{
		_userRepository = userRepository;
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

	private async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByIdAsync(userId, RequestContext.OrganizationId, cancellationToken);
		if (user == null)
		{
			throw new AppBusinessException($"User [Id={userId}] does not exist");
		}

		return user;
	}
}