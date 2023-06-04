namespace SLCP.ServiceModel;

public class AccessLog
{
	public Guid Id { get; set; }

	public Lock Lock { get; set; }

	public AccessTag Tag { get; set; }

	public User User { get; set; }

	public DateTimeOffset AccessedDateTime { get; set; }

	public AccessState AccessState { get; set; }
}

public enum AccessState
{
	Success,
	Failed
}