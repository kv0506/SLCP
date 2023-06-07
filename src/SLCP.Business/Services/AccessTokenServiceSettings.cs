namespace SLCP.Business.Services;

public class AccessTokenServiceSettings
{
	public const string SectionName = "AccessTokenServiceSettings";

	public string Secret { get; set; }
	public int ExpiryInMinutes { get; set; }
}