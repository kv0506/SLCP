namespace SLCP.Business.Request;

public class ValidateLockAccessUsingAccessCodeCommand : ValidateLockAccessCommand
{
	public string? UserLockAccessCode { get; set; }
}