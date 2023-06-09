namespace SLCP.ServiceModel;

public class User
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string EmailAddress { get; set; }

	public string PasswordHash { get; set; }

	public string Salt { get; set; }

	public string Role { get; set; }

	public string LockAccessCodeHash { get; set; }

	public IList<Guid> Tags { get; set; }

	public IList<Guid> Locations { get; set; }

	public Guid OrganizationId { get; set; }
}