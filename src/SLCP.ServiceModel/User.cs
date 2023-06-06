namespace SLCP.ServiceModel;

public class User
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string EmailAddress { get; set; }

	public string PasswordHash { get; set; }

	public string PasswordSalt { get; set; }

	public string LockAccessCode { get; set; }

	public IList<LockGroup> PermittedLockGroups { get; set; }

	public Organization Organization { get; set; }
}