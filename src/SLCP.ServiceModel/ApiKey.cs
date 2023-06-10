namespace SLCP.ServiceModel;

public class ApiKey
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Key { get; set; }

	public Guid LocationId { get; set; }

	public Guid OrganizationId { get; set; }
}