using MediatR;
using SLCP.Business.Request;
using SLCP.DataAccess;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class GetLockAccessLogQueryHandler : IRequestHandler<GetLockAccessLogQuery, QueryResult<LockAccessLog>>
{
	private readonly ILockAccessLogRepository _lockAccessLogRepository;

	public GetLockAccessLogQueryHandler(ILockAccessLogRepository lockAccessLogRepository)
	{
		_lockAccessLogRepository = lockAccessLogRepository;
	}

	public async Task<QueryResult<LockAccessLog>> Handle(GetLockAccessLogQuery request, CancellationToken cancellationToken)
	{
		return await _lockAccessLogRepository.GetItemsAsync(request.LockId, request.UserId, request.LocationId,
			request.PageSize, request.ContinuationToken, cancellationToken);
	}
}