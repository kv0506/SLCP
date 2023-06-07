using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SLCP.Business.Exception;
using SLCP.ServiceModel;

namespace SLCP.Business.Services;

public interface IAccessTokenService
{
	string CreateToken(User user, string secret, int expiryInMinutes);
	User DecodeToken(string jwtToken);
}

public class AccessTokenService : IAccessTokenService
{
	private const string OrganizationClaim = "Organization";

	public string CreateToken(User user, string secret, int expiryInMinutes)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(secret);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString("D")),
				new Claim(ClaimTypes.Email, user.EmailAddress),
				new Claim(OrganizationClaim, user.OrganizationId.ToString("D"))
			}),
			Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var jwtToken = tokenHandler.WriteToken(token);

		return jwtToken;
	}

	public User DecodeToken(string jwtToken)
	{
		var tokenHandler = new JwtSecurityTokenHandler();

		if (tokenHandler.CanReadToken(jwtToken))
		{
			var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);

			var claims = jwtSecurityToken.Claims;
			var user = new User();

			foreach (var claim in claims)
			{
				switch (claim.Type)
				{
					case ClaimTypes.NameIdentifier:
						user.Id = Guid.Parse(claim.Value);
						break;
					case ClaimTypes.Email:
						user.EmailAddress = claim.Value;
						break;
					case OrganizationClaim:
						user.OrganizationId = Guid.Parse(claim.Value);
						break;
				}
			}

			return user;
		}

		throw new AppBusinessException($"Invalid token {jwtToken}");
	}
}