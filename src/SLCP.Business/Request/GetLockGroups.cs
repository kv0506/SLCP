using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Request;

public class GetLockGroups :  IRequest<IList<LockGroup>>
{
	public Guid LockId { get; set; }
}