namespace SLCP.Business.Services;

public interface IRequestContext
{
	public Guid? UserId { get; set; }
	public string? UserRole { get; set; }
	public Guid? ApiKeyId { get; set; }
	public IList<Guid> Locations { get; set; }
	public Guid OrganizationId { get; set; }
}

public class RequestContext : IRequestContext
{
	public Guid? UserId { get; set; }
	public string? UserRole { get; set; }
	public Guid? ApiKeyId { get; set; }
	public IList<Guid> Locations { get; set; }
	public Guid OrganizationId { get; set; }
}