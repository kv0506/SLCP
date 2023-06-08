using MediatR;
using Microsoft.AspNetCore.Mvc;
using SLCP.API.Security.Attributes;
using SLCP.Business.Request;
using SLCP.Core;

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
		[AuthorizedRoles(Roles.Employee, Roles.SecurityAdmin)]
		[Route("ValidateByAccessCode")]
		public async Task<IActionResult> Validate([FromBody] ValidateLockAccessUsingAccessCodeCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}