namespace SLCP.API.Exception;

public class AppModelValidationException : System.Exception
{
	public AppModelValidationException(string message) : base(message)
	{
		Errors = new List<string>();
	}

	public List<string> Errors { get; }
}