namespace SLCP.API.Security.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AllowKeyBasedAuthAttribute : Attribute
{
	public AllowKeyBasedAuthAttribute()
	{
	}
}