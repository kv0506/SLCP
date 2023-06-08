namespace SLCP.Core;

public static class GuidExtensions
{
	public static string ToHyphens(this Guid guid) => guid.ToString("D");
}