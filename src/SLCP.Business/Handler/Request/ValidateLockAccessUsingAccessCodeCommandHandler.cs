using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessCodeCommandHandler : ValidateLockAccessCommandHandler,
	IRequestHandler<ValidateLockAccessUsingAccessCodeCommand, LockAccessResponse>
{
	private readonly IUserRepository _userRepository;

	public ValidateLockAccessUsingAccessCodeCommandHandler(IUserRepository userRepository,
		IUserAccessGroupRepository userAccessGroupRepository, IMediator mediator, IRequestContext requestContext) :
		base(userAccessGroupRepository, mediator, requestContext)
	{
		_userRepository = userRepository;
	}

	public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessCodeCommand request,
		CancellationToken cancellationToken)
	{
		var user = await GetUserAsync(RequestContext.UserId.GetValueOrDefault(), cancellationToken);

		if (!HashService.VerifyHash(request.UserLockAccessCode, user.Salt, user.LockAccessCodeHash))
		{
			await PublishLockAccessEvent(new Lock
				{
					Id = request.LockId, LocationId = request.LocationId, OrganizationId = RequestContext.OrganizationId
				}, user, AccessState.Denied, AccessDeniedReason.InvalidUserLockAccessCode, cancellationToken);

			return AccessDenied(ErrorCode.InvalidAccessCode, "Lock access code is invalid");
		}

		return await DoesUserHaveAccessForLockAsync(request, user, cancellationToken);
	}

	private async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByIdAsync(userId, RequestContext.OrganizationId, cancellationToken);
		if (user == null)
		{
			throw new AppException(ErrorCode.NotFound, $"User [Id={userId}] does not exist");
		}

		return user;
	}
}