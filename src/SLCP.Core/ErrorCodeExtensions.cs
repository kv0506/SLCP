namespace SLCP.Core;

public static class ErrorCodeExtensions
{
	public static int ToHttpStatusCode(this ErrorCode errorCode)
	{
		switch (errorCode)
		{
			case ErrorCode.NotFound:
				return 404;
			case ErrorCode.Unauthorized:
			case ErrorCode.AccessTokenExpired:
			case ErrorCode.DoesNotHaveAccessToLock:
				return 401;
		}

		return 400;
	}
}