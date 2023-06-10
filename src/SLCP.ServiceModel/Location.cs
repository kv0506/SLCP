namespace SLCP.ServiceModel;

public class Location
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public IList<Lock> Locks { get; set; }

	public Guid OrganizationId { get; set; }
}