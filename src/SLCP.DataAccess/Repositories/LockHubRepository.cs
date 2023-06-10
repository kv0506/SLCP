using SLCP.Core;
using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockHubRepository : ILockHubRepository
{
	private readonly ICosmosService _cosmosService;

	public LockHubRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<LockHub> GetByLockIdAsync(Guid lockId, Guid locationId, CancellationToken cancellationToken)
	{
		var query = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.locks, {id: '" + lockId.ToHyphens() +
		            "'}, true)";
		var lockHubs = await _cosmosService.GetItemsAsync<LockHub>(ContainerNames.LockHubs, query, locationId.ToHyphens(), cancellationToken);
		return lockHubs.SingleOrDefault();
	}
}