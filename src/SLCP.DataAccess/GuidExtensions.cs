namespace SLCP.DataAccess;

public static class GuidExtensions
{
	public static string ToHyphens(this Guid guid) => guid.ToString("D");
}