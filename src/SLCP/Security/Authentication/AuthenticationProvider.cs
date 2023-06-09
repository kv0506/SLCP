﻿using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Headers;
using CSharpExtensions;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.API.Security.Authentication;

public class AuthenticationProvider : IAuthenticationProvider
{
	public const string DefaultScheme = "Bearer";
	public const string ApiKeyScheme = "ApiKey";

	private readonly IAccessTokenService _accessTokenService;
	private readonly IApiKeyRepository _apiKeyRepository;
	private readonly IRequestContext _requestContext;
	private readonly ILogger<AuthenticationProvider> _logger;

	public AuthenticationProvider(IAccessTokenService accessTokenService, IApiKeyRepository apiKeyRepository,
		IRequestContext requestContext, ILogger<AuthenticationProvider> logger)
	{
		_accessTokenService = accessTokenService;
		_apiKeyRepository = apiKeyRepository;
		_requestContext = requestContext;
		_logger = logger;
	}

	public async Task AuthenticateAsync(ActionExecutingContext context, CancellationToken cancellationToken)
	{
		await HandleAuthenticate(context.HttpContext, cancellationToken).ConfigureAwait(false);
	}

	private async Task HandleAuthenticate(HttpContext context, CancellationToken cancellationToken)
	{
		if (!context.Request.Headers.ContainsKey("Authorization"))
			throw AuthenticationFailed("Authorization header is missing");

		var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
		var authHeaderParts = authHeader.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		if (authHeaderParts.Length != 2)
		{
			throw AuthenticationFailed("Authorization header is in invalid format");
		}

		var authHeaderScheme = authHeaderParts.Length >= 1 ? authHeaderParts[0] : string.Empty;
		var authHeaderValue = authHeaderParts.Length >= 2 ? authHeaderParts[1] : string.Empty;

		if (DefaultScheme.IsNotEquals(authHeaderScheme, StringComparison.OrdinalIgnoreCase) &&
		    ApiKeyScheme.IsNotEquals(authHeaderScheme, StringComparison.OrdinalIgnoreCase))
		{
			throw AuthenticationFailed("Authorization scheme is not supported");
		}

		if (authHeaderValue.IsNullOrWhiteSpace())
		{
			throw AuthenticationFailed($"{authHeaderScheme} Token is missing");
		}

		if (DefaultScheme.IsEquals(authHeaderScheme, StringComparison.OrdinalIgnoreCase))
		{
			var user = _accessTokenService.DecodeToken(authHeaderValue);
			if (user == null)
			{
				throw AuthenticationFailed("Bearer Token is not valid");
			}

			_requestContext.UserId = user.Id;
			_requestContext.UserRole = user.Role;
			_requestContext.Locations = user.Locations;
			_requestContext.OrganizationId = user.OrganizationId;
		}
		else
		{
			ApiKey apiKey = null;

			try
			{
				apiKey = await _apiKeyRepository.GetByKeyAsync(authHeaderValue, cancellationToken);
			}
			catch (AppException e)
			{
				_logger.LogError(e.Message, e);
			}

			if (apiKey != null)
			{
				_requestContext.ApiKeyId = apiKey.Id;
				_requestContext.Locations = new List<Guid> { apiKey.LocationId };
				_requestContext.OrganizationId = apiKey.OrganizationId;
			}
			else
			{
				throw AuthenticationFailed("Invalid ApiKey");
			}
		}
	}

	private AuthenticationException AuthenticationFailed(string errorMessage)
	{
		return new AuthenticationException(ErrorCode.Unauthorized, errorMessage);
	}
}