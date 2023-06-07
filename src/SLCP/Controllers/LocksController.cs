using MediatR;
using Microsoft.AspNetCore.Mvc;
using SLCP.API.Security.Attributes;
using SLCP.Business.Request;

namespace SLCP.API.Controllers
{
	[Route("api/[controller]")]
	public class LocksController : AppControllerBase
	{
		private readonly IMediator _mediator;

		public LocksController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[AllowKeyBasedAuth]
		[Route("ValidateByAccessTag")]
		public async Task<IActionResult> Validate([FromBody] ValidateLockAccessUsingAccessTagCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}

		[HttpPost]
		[AuthorizedRoles("Employee")]
		[Route("ValidateByAccessCode")]
		public async Task<IActionResult> Validate([FromBody] ValidateLockAccessUsingAccessCodeCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}