using Microsoft.AspNetCore.Mvc.Filters;

namespace SLCP.API.Security.Authentication;

public interface IAuthenticationProvider
{
	/// <summary>
	/// Validates the bearer token in the request and loads the associated user's data into IRequestContent
	/// </summary>
	Task AuthenticateAsync(ActionExecutingContext context, CancellationToken cancellationToken);
}