using Microsoft.AspNetCore.Mvc;
using SLCP.API.Model;

namespace SLCP.API.Controllers;

[ApiController]
public class AppControllerBase : ControllerBase
{
	protected ApiResponse Success(object result) => new ApiResponse { IsSuccess = true, Result = result };

	protected ApiResponse Error(string error) =>
		new ApiResponse { IsSuccess = false, Errors = new List<string> { error } };
}