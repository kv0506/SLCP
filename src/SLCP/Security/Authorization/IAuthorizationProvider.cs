using Microsoft.AspNetCore.Mvc.Filters;

namespace SLCP.API.Security.Authorization;

public interface IAuthorizationProvider
{
	/// <summary>
	/// Validates whether the authenticated user has the required role to call the action
	/// </summary>
	Task AuthorizeAsync(ActionExecutingContext context, CancellationToken cancellationToken);
}