using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class AccessTagRepository : IAccessTagRepository
{
	private const string ContainerName = "access-tags";

	private readonly ICosmosService _cosmosService;

	public AccessTagRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<AccessTag> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
	{
		return await _cosmosService.GetItemAsync<AccessTag>(ContainerName, id.ToString("D"), orgId?.ToString("D"),
			cancellationToken);
	}
}