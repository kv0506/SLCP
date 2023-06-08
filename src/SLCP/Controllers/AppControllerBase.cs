using Microsoft.AspNetCore.Mvc;
using SLCP.API.Model;

namespace SLCP.API.Controllers;

[ApiController]
public class AppControllerBase : ControllerBase
{
	protected ApiResponse Success(object result) => new ApiResponse { IsSuccess = true, Result = result };
}