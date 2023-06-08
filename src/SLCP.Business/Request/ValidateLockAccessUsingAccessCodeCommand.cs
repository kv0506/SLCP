using MediatR;
using SLCP.Business.Response;

namespace SLCP.Business.Request;

public class ValidateLockAccessUsingAccessCodeCommand : IRequest<LockAccessResponse>
{
	public Guid LockId { get; set; }

	public string? UserLockAccessCode { get; set; }
}