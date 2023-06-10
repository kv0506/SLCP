using MediatR;
using Microsoft.AspNetCore.Mvc;
using SLCP.API.Security.Attributes;
using SLCP.Business.Request;
using SLCP.Core;

namespace SLCP.API.Controllers
{
	[Route("api/locations/{locationId}/[controller]")]
	public class LockAccessLogsController : AppControllerBase
	{
		private readonly IMediator _mediator;

		public LockAccessLogsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		[AuthorizedRoles(Roles.SecurityAdmin)]
		public async Task<IActionResult> Get([FromRoute] Guid locationId, [FromQuery] GetLockAccessLogQuery request)
		{
			//workaround. ideally we need a FromRouteAndQuery binding option
			request.LocationId = locationId;
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}