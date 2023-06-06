using MediatR;
using Microsoft.AspNetCore.Mvc;
using SLCP.Business.Request;

namespace SLCP.API.Controllers
{
	[Route("api/[controller]")]
	public class LockAccessLogsController : AppControllerBase
	{
		private readonly IMediator _mediator;

		public LockAccessLogsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] GetLockAccessLogQuery request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}