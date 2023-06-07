namespace SLCP.API.Security;

public class AuthenticationException : System.Exception
{
	public AuthenticationException(string message) : base(message)
	{
	}
}