using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Request;

public class CreateLockAccessLogCommand : IRequest
{
	public Lock Lock { get; set; }

	public User User { get; set; }

	public AccessState AccessState { get; set; }

	public AccessDeniedReason? AccessDeniedReason { get; set; }
}