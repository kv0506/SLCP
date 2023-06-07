using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockGroupRepository : ILockGroupRepository
{
    private readonly ICosmosService _cosmosService;

    public LockGroupRepository(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<LockGroup> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
    {
		return await _cosmosService.GetItemAsync<LockGroup>(ContainerNames.LockGroups, id.ToHyphens(), orgId?.ToHyphens(),
			cancellationToken);
	}

    public async Task<IList<LockGroup>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.locks, {id: '" + lockId.ToHyphens() +
                    "'}, true)";
        return await _cosmosService.GetItemsAsync<LockGroup>(ContainerNames.LockGroups, query, orgId?.ToHyphens(), cancellationToken);
    }
}