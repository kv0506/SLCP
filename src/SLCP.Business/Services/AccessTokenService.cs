using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SLCP.Core;
using SLCP.ServiceModel;

namespace SLCP.Business.Services;

public interface IAccessTokenService
{
	string CreateToken(User user);
	User? DecodeToken(string jwtToken);
}

public class AccessTokenService : IAccessTokenService
{
	private const string Id = "id";
	private const string Role = "role";
	private const string OrganizationClaim = "organization";

	private readonly AccessTokenServiceSettings _settings;

	public AccessTokenService(AccessTokenServiceSettings settings)
	{
		_settings = settings;
	}

	public string CreateToken(User user)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_settings.Secret);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim(Id, user.Id.ToHyphens()),
				new Claim(Role, user.Role),
				new Claim(OrganizationClaim, user.OrganizationId.ToHyphens())
			}),
			Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryInMinutes),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var jwtToken = tokenHandler.WriteToken(token);

		return jwtToken;
	}

	public User? DecodeToken(string jwtToken)
	{
		var tokenHandler = new JwtSecurityTokenHandler();

		if (tokenHandler.CanReadToken(jwtToken))
		{
			var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);

			if (jwtSecurityToken.ValidTo < DateTime.UtcNow)
				throw new AppException(ErrorCode.AccessTokenExpired,
					"Access token is expired. Please request for a new token");

			var claims = jwtSecurityToken.Claims;
			var user = new User();

			foreach (var claim in claims)
			{
				switch (claim.Type)
				{
					case Id:
						user.Id = Guid.Parse(claim.Value);
						break;
					case Role:
						user.Role = claim.Value;
						break;
					case OrganizationClaim:
						user.OrganizationId = Guid.Parse(claim.Value);
						break;
				}
			}

			return user;
		}

		return null;
	}
}