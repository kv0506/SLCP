namespace SLCP.API.Security.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class AuthorizedRolesAttribute : Attribute
{
	public AuthorizedRolesAttribute(params string[] roles)
	{
		if (roles.Length == 0)
			throw new ArgumentNullException(nameof(roles), "Roles can not be null or empty");

		Roles = roles;
	}

	public string[] Roles { get; }
}