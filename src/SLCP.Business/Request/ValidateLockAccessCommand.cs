using MediatR;
using SLCP.Business.Response;

namespace SLCP.Business.Request;

public class ValidateLockAccessCommand : IRequest<LockAccessResponse>
{
	public Guid LockId { get; set; }

	public Guid LocationId { get; set; }
}