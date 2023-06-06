namespace SLCP.DataAccess.CosmosService;

public class CosmosSettings
{
	public const string SectionName = "CosmosSettings";

	public string ConnectionString { get; set; }
	public string DatabaseName { get; set; }
}