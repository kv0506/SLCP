using CSharpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using SLCP.API.Security.Attributes;
using SLCP.Business.Services;

namespace SLCP.API.Security.Authorization;

public class AuthorizationProvider : IAuthorizationProvider
{
	private readonly IRequestContext _requestContext;

	public AuthorizationProvider(IRequestContext requestContext)
	{
		_requestContext = requestContext;
	}

	public async Task AuthorizeAsync(ActionExecutingContext context, CancellationToken cancellationToken)
	{
		if (_requestContext.UserId.IsNotNullOrEmpty())
		{
			var allowAnonymousAttribute = context.ActionDescriptor.GetCustomAttributeFromActionOrController<AllowAnonymousAttribute>(true);
			if (allowAnonymousAttribute != null)
			{
				return;
			}

			// verify whether user has the role as mentioned in the api
			var authorizedRolesAttribute = context.ActionDescriptor.GetCustomAttributeFromActionOrController<AuthorizedRolesAttribute>(true);
			if (authorizedRolesAttribute != null && authorizedRolesAttribute.Roles.Any(x => x == _requestContext.UserRole))
			{
				return;
			}

			throw new AuthenticationException("You are not authorized to perform this operation");
		}

		if (_requestContext.ApiKeyId.IsNotNullOrEmpty())
		{
			// verify whether the api is allowed for key based auth
			var keyBasedAuthAttribute = context.ActionDescriptor.GetCustomAttributeFromActionOrController<AllowKeyBasedAuthAttribute>(true);
			if (keyBasedAuthAttribute != null)
			{
				return;
			}

			throw new AuthenticationException("This api is not allowed for key based authentication");
		}
	}
}