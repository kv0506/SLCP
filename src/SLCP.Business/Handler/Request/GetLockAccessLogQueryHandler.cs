using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Services;
using SLCP.DataAccess;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class GetLockAccessLogQueryHandler : IRequestHandler<GetLockAccessLogQuery, QueryResult<LockAccessLog>>
{
	private readonly ILockAccessLogRepository _lockAccessLogRepository;
	private readonly IRequestContext _requestContext;

	public GetLockAccessLogQueryHandler(ILockAccessLogRepository lockAccessLogRepository, IRequestContext requestContext)
	{
		_lockAccessLogRepository = lockAccessLogRepository;
		_requestContext = requestContext;
	}

	public async Task<QueryResult<LockAccessLog>> Handle(GetLockAccessLogQuery request, CancellationToken cancellationToken)
	{
		return await _lockAccessLogRepository.GetItemsAsync(request.LockId, request.UserId, _requestContext.OrganizationId,
			request.PageSize, request.ContinuationToken, cancellationToken);
	}
}