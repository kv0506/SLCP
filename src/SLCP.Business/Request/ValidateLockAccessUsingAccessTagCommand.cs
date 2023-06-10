namespace SLCP.Business.Request;

public class ValidateLockAccessUsingAccessTagCommand : ValidateLockAccessCommand
{
	public Guid AccessTagId { get; set; }
}