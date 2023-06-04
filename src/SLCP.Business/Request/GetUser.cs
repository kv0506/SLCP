using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Request;

public class GetUser : IRequest<User>
{
	public Guid Id { get; set; }
}