﻿using MediatR;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;

namespace SLCP.Business.Handler.Request;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
	private readonly IUserRepository _userRepository;
	private readonly IAccessTokenService _accessTokenService;

	public LoginCommandHandler(IUserRepository userRepository, IAccessTokenService accessTokenService)
	{
		_userRepository = userRepository;
		_accessTokenService = accessTokenService;
	}

	public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByEmailAsync(request.Email, null, cancellationToken);
		if (user == null)
		{
			throw new AppException(ErrorCode.InvalidUsernameOrPassword, "Invalid email address or password");
		}

		if (HashService.VerifyHash(request.Password, user.Salt, user.PasswordHash))
		{
			var accessToken = _accessTokenService.CreateToken(user);
			return new LoginResponse
			{
				AccessToken = accessToken
			};
		}

		throw new AppException(ErrorCode.InvalidUsernameOrPassword, "Invalid email address or password");
	}
}