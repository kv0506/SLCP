using System.ComponentModel.DataAnnotations;
using MediatR;
using SLCP.Business.Response;

namespace SLCP.Business.Request;

public class LoginCommand : IRequest<LoginResponse>
{
	[Required]
	public string Email { get; set; }

	[Required]
	public string Password { get; set; }
}