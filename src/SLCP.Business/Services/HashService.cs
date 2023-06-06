using CSharpExtensions;
using System.Security.Cryptography;
using System.Text;

namespace SLCP.Business.Services;

public class HashService
{
	private static readonly int MinimumSaltLength = 16;

	public static string CreateSalt()
	{
		var salt = new StringBuilder();
		while (salt.Length < MinimumSaltLength)
		{
			salt.Append(RandomNumberGenerator.GetInt32(int.MaxValue));
		}

		return salt.ToString();
	}

	public static string CreateHash(string input, string salt)
	{
		var contentBytes = Encoding.UTF8.GetBytes($"{input}{salt}");
		var hashBytes = SHA256.Create().ComputeHash(contentBytes);
		return Convert.ToBase64String(hashBytes);
	}

	public static bool VerifyHash(string input, string salt, string hash)
	{
		var contentBytes = Encoding.UTF8.GetBytes($"{input}{salt}");
		var hashBytes = SHA256.Create().ComputeHash(contentBytes);
		var hashString = Convert.ToBase64String(hashBytes);

		return hashString.IsNotNullOrEmptyAndEquals(hash);
	}
}