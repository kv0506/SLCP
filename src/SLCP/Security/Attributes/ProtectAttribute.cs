using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using SLCP.API.Security.Authentication;
using SLCP.API.Security.Authorization;

namespace SLCP.API.Security.Attributes;

/// <summary>
/// Runs authentication/authorization checks for every action
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class ProtectAttribute : Attribute, IActionFilter, IAsyncActionFilter
{
	private readonly IAuthenticationProvider _authenticationProvider;
	private readonly IAuthorizationProvider _authorizationProvider;

	public ProtectAttribute(IAuthenticationProvider authenticationProvider, IAuthorizationProvider authorizationProvider)
	{
		_authenticationProvider = authenticationProvider;
		_authorizationProvider = authorizationProvider;
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}

	public void OnActionExecuting(ActionExecutingContext context)
	{
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		await OnActionExecutingAsync(context).ConfigureAwait(false);
		if (context.Result == null)
		{
			OnActionExecuted(await next().ConfigureAwait(false));
		}
	}

	private async Task OnActionExecutingAsync(ActionExecutingContext context)
	{
		if (context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() == null)
		{
			await _authenticationProvider.AuthenticateAsync(context, CancellationToken.None).ConfigureAwait(false);
			await _authorizationProvider.AuthorizeAsync(context, CancellationToken.None).ConfigureAwait(false);
		}
	}
}