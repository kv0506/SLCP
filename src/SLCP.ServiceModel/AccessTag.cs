namespace SLCP.ServiceModel;

public class AccessTag
{
	public Guid Id { get; set; }

	public string Number { get; set; }

	public bool IsBlocked { get; set; }

	public User User { get; set; }

	public Guid LocationId { get; set; }

	public Guid OrganizationId { get; set; }
}