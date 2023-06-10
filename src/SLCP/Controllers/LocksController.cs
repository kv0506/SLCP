using MediatR;
using Microsoft.AspNetCore.Mvc;
using SLCP.API.ModelBinding;
using SLCP.API.Security.Attributes;
using SLCP.Business.Request;
using SLCP.Core;

namespace SLCP.API.Controllers
{
	[Route("api/locations/{locationId}/[controller]")]
	public class LocksController : AppControllerBase
	{
		private readonly IMediator _mediator;

		public LocksController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		[AllowKeyBasedAuth]
		[Route("{LockId}/access-tag")]
		public async Task<IActionResult> Validate([FromBodyAndRoute] ValidateLockAccessUsingAccessTagCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}

		[HttpPost]
		[AuthorizedRoles(Roles.Employee, Roles.SecurityAdmin, Roles.SystemAdmin)]
		[Route("{LockId}/access-code")]
		public async Task<IActionResult> Validate([FromBodyAndRoute] ValidateLockAccessUsingAccessCodeCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}