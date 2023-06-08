using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class ValidateLockAccessUsingAccessTagCommandHandler : ValidateLockAccessCommandHandler,
	IRequestHandler<ValidateLockAccessUsingAccessTagCommand, LockAccessResponse>
{
	private readonly IAccessTagRepository _accessTagRepository;

	public ValidateLockAccessUsingAccessTagCommandHandler(ILockRepository lockRepository,
		IAccessTagRepository accessTagRepository,
		IMediator mediator, IRequestContext requestContext) : base(lockRepository, mediator, requestContext)
	{
		_accessTagRepository = accessTagRepository;
	}

	public async Task<LockAccessResponse> Handle(ValidateLockAccessUsingAccessTagCommand request,
		CancellationToken cancellationToken)
	{
		var lockObj = await GetLockAsync(request.LockId, cancellationToken);

		var accessTag = await GetAccessTagAsync(request.AccessTagId, RequestContext.OrganizationId,
			cancellationToken);

		if (accessTag.IsBlocked)
		{
			await PublishLockAccessEvent(lockObj, accessTag.User, AccessState.Denied,
				AccessDeniedReason.AccessTagBlocked, cancellationToken);
			return AccessDenied(ErrorCode.AccessTagIsBlocked, $"AccessTag [Id={request.AccessTagId}] is blocked");
		}

		return await DoesUserHaveAccessForLockAsync(lockObj, accessTag.User, cancellationToken);
	}

	private async Task<AccessTag> GetAccessTagAsync(Guid accessTagId, Guid orgId, CancellationToken cancellationToken)
	{
		var accessTag = await _accessTagRepository.GetByIdAsync(accessTagId, orgId, cancellationToken);

		if (accessTag == null)
		{
			throw new AppException(ErrorCode.NotFound, $"AccessTag [Id={accessTagId}] does not exist");
		}

		return accessTag;
	}
}