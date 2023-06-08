using SLCP.Core;

namespace SLCP.API.Security;

public class AuthenticationException : AppException
{
	public AuthenticationException(ErrorCode errorCode, string message) : base(errorCode, message)
	{
	}
}