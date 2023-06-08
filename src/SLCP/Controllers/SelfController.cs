using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCP.Business.Request;

namespace SLCP.API.Controllers
{
	[Route("api/[controller]")]
	public class SelfController : AppControllerBase
	{
		private readonly IMediator _mediator;

		public SelfController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("login")]
		public async Task<IActionResult> SignIn([FromBody] LoginCommand request)
		{
			var result = await _mediator.Send(request);
			return Ok(result);
		}
	}
}
