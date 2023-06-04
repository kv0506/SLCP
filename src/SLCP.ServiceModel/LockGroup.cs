namespace SLCP.ServiceModel;

public class LockGroup
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public IList<Lock> Locks { get; set; }

	public Organization Organization { get; set; }
}