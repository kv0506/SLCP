using MediatR;
using SLCP.Business.Response;

namespace SLCP.Business.Request;

public class ValidateLockAccessUsingAccessTagCommand : IRequest<LockAccessResponse>
{
	public Guid LockId { get; set; }

	public Guid AccessTagId { get; set; }
}