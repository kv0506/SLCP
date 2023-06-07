using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
	private readonly ICosmosService _cosmosService;

	public ApiKeyRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<ApiKey> GetByKeyAsync(string key, CancellationToken cancellationToken)
	{
		var query = $"SELECT * FROM c WHERE c.key = '{key}'";
		return await _cosmosService.GetItemAsync<ApiKey>(ContainerNames.ApiKey, query, cancellationToken);
	}
}