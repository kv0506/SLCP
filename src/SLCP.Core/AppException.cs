namespace SLCP.Core;

public class AppException : Exception
{
	public AppException(ErrorCode errorCode, string message) : this(errorCode, message, null)
	{
	}

	public AppException(ErrorCode errorCode, string message, Exception exception) : base(message,
		exception)
	{
		ErrorCode = errorCode;
	}

	public ErrorCode ErrorCode { get; set; }
}