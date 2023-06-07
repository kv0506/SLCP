namespace SLCP.Business.Exception;

public class AppBusinessException : System.Exception
{
	public AppBusinessException(string message) : base(message)
	{
	}

	public AppBusinessException(string message, System.Exception exception) : base(message, exception)
	{
	}
}