namespace SLCP.DataAccess.Exception;

public class AppDomainException : System.Exception
{
	public AppDomainException(string message) : base(message)
	{
	}

	public AppDomainException(string message, System.Exception exception) : base(message, exception)
	{
	}
}