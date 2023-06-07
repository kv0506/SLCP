using MediatR;
using SLCP.Business.Response;

namespace SLCP.Business.Request;

public class LoginCommand : IRequest<LoginResponse>
{
	public string Email { get; set; }

	public string Password { get; set; }
}