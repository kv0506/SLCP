using CSharpExtensions;

namespace SLCP.Core;

public static class StringExtensions
{
	public static string ToBase64(this string value)
	{
		if (value.IsNotNullOrEmpty())
		{
			var textBytes = System.Text.Encoding.UTF8.GetBytes(value);
			return Convert.ToBase64String(textBytes);
		}

		return string.Empty;
	}

	public static string FromBase64(this string value)
	{
		if (value.IsNotNullOrEmpty())
		{
			var base64EncodedBytes = Convert.FromBase64String(value);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}

		return string.Empty;
	}
}