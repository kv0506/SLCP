﻿using CSharpExtensions;
using Microsoft.AspNetCore.Mvc;
using SLCP.API.Exception;
using SLCP.API.Model;

namespace SLCP.API.Controllers;

[ApiController]
public class AppControllerBase : ControllerBase
{
	protected ApiResponse Success(object result) => new ApiResponse { IsSuccess = true, Result = result };

	protected ApiResponse Error(string error) =>
		new ApiResponse { IsSuccess = false, Errors = new List<string> { error } };

	protected IActionResult Error()
	{
		var validationException = new AppModelValidationException("Operation failed with validation issues");

		foreach (var modelState in ModelState)
		{
			if (modelState.Key.IsNotNullOrWhiteSpace())
			{
				foreach (var err in modelState.Value.Errors)
				{
					validationException.Errors.Add(err.ErrorMessage.IsNotNullOrWhiteSpace()
						? err.ErrorMessage
						: err.Exception?.Message);
				}
			}
		}

		throw validationException;
	}
}