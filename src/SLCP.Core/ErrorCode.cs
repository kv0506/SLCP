namespace SLCP.Core
{
	public enum ErrorCode
	{
		UnknownError,
		DatabaseError,
		Unauthorized,
		NotFound,
		AccessTokenExpired,
		InvalidUsernameOrPassword,
		InvalidAccessCode,
		AccessTagIsBlocked,
		DoesNotHaveAccessToLock
	}
}