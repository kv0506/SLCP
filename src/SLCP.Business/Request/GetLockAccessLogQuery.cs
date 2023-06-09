using MediatR;
using SLCP.DataAccess;
using SLCP.ServiceModel;

namespace SLCP.Business.Request;

public class GetLockAccessLogQuery : IRequest<QueryResult<LockAccessLog>>
{
	public Guid LocationId { get; set; }

	public Guid? UserId { get; set; }

	public Guid? LockId { get; set; }

	public int PageSize { get; set; }

	public string? ContinuationToken { get; set; }
}