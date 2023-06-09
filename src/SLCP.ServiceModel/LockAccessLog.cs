namespace SLCP.ServiceModel;

public class LockAccessLog
{
	public Guid Id { get; set; }

	public Lock Lock { get; set; }

	public User User { get; set; }

	public DateTimeOffset AccessedDateTime { get; set; }

	public AccessState AccessState { get; set; }

	public AccessDeniedReason? AccessDeniedReason { get; set; }

	public Guid LocationId { get; set; }

	public Guid OrganizationId { get; set; }
}

public enum AccessState
{
	Allowed,
	Denied
}

public enum AccessDeniedReason
{
	InvalidUserLockAccessCode,
	AccessTagBlocked,
	DoesNotHaveAccessToLock
}