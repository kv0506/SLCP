using MediatR;
using SLCP.Business.Request;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler;

public class GetUserHandler : IRequestHandler<GetUser, User>
{
	public async Task<User> Handle(GetUser request, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}