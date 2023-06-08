using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Request;

public class OpenLockCommand : IRequest
{
	public Lock Lock { get; set; }
}