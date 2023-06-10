using CSharpExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using SLCP.API.Security.Attributes;
using SLCP.Business.Services;
using SLCP.Core;

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
		ValidateRole(context);
		ValidateLocation(context);
	}

	private void ValidateRole(ActionExecutingContext context)
	{
		if (_requestContext.UserId.IsNotNullOrEmpty())
		{
			var allowAnonymousAttribute =
				context.ActionDescriptor.GetCustomAttributeFromActionOrController<AllowAnonymousAttribute>(true);
			if (allowAnonymousAttribute != null)
			{
				return;
			}

			// verify whether user has the role as mentioned in the api
			var authorizedRolesAttribute =
				context.ActionDescriptor.GetCustomAttributeFromActionOrController<AuthorizedRolesAttribute>(true);
			if (authorizedRolesAttribute != null &&
			    authorizedRolesAttribute.Roles.Any(x => x == _requestContext.UserRole))
			{
				return;
			}

			throw new AuthenticationException(ErrorCode.Unauthorized,
				"You are not authorized to perform this operation");
		}

		if (_requestContext.ApiKeyId.IsNotNullOrEmpty())
		{
			// verify whether the api is allowed for key based auth
			var keyBasedAuthAttribute =
				context.ActionDescriptor.GetCustomAttributeFromActionOrController<AllowKeyBasedAuthAttribute>(true);
			if (keyBasedAuthAttribute != null)
			{
				return;
			}

			throw new AuthenticationException(ErrorCode.Unauthorized, "This api is not allowed for key based authentication");
		}
	}

	public void ValidateLocation(ActionExecutingContext context)
	{
		// ignore identifier check for admin since they can crud on all records
		if (_requestContext.UserRole == Roles.SystemAdmin)
			return;

		// add the authenticated locationIds to authorized list. every user can act on their own record
		var authorizedIdentifiers = _requestContext.Locations;

		// get all location ids which needs to be authorized
		var locationIds = GetLocationIdentifiers(context.ActionArguments);

		foreach (var locationId in locationIds)
		{
			if (locationId.IsNotNullOrEmpty() && !(authorizedIdentifiers?.Contains(locationId.GetValueOrDefault()) ?? false))
			{
				throw new AuthenticationException(ErrorCode.Unauthorized,
					"You do not have access to locations mentioned in the request");
			}
		}
	}

	private List<Guid?> GetLocationIdentifiers(IDictionary<string, object> actionParameters)
	{
		var locationIds = new List<Guid?>();

		foreach (var parameter in actionParameters)
		{
			if (parameter.Value == null)
				continue;

			if (parameter.Key.IsEquals("locationId", StringComparison.InvariantCultureIgnoreCase))
			{
				locationIds.Add(Guid.TryParse(parameter.Value.ToString(), out var id) ? id : (Guid?)null);
			}

			if (parameter.Value.GetType().IsClass)
			{
				var properties = parameter.Value.GetType().GetProperties();
				foreach (var property in properties)
				{
					if (property.Name.EndsWith("locationId", StringComparison.InvariantCultureIgnoreCase))
					{
						var propertyValue = property.GetValue(parameter.Value);
						if (propertyValue != null)
						{
							locationIds.Add(Guid.TryParse(propertyValue.ToString(), out var id) ? id : (Guid?)null);
						}
					}
				}
			}
		}

		return locationIds;
	}
}