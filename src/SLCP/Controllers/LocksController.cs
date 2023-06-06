﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> Validate([FromBody] ValidateLockAccessUsingAccessTagCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}

		[HttpPost]
		public async Task<IActionResult> Validate([FromBody] ValidateLockAccessUsingAccessCodeCommand request)
		{
			return Ok(Success(await _mediator.Send(request)));
		}
	}
}
