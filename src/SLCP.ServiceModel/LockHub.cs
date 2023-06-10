namespace SLCP.ServiceModel;

public class LockHub
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public IList<Lock> Locks { get; set; }

	public Guid LocationId { get; set; }

	public Guid OrganizationId { get; set; }
}