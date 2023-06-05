using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockGroupRepository : ILockGroupRepository
{
    private const string ContainerName = "lock-groups";

    private readonly ICosmosService _cosmosService;

    public LockGroupRepository(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<LockGroup> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
    {
		return await _cosmosService.GetItemAsync<LockGroup>(ContainerName, id.ToString("D"), orgId?.ToString("D"),
			cancellationToken);
	}

    public async Task<IList<LockGroup>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM lock-groups lg WHERE ARRAY_CONTAINS(lg.locks, {\"id\": " + lockId.ToString("D") +
                    "})";
        return await _cosmosService.GetItemsAsync<LockGroup>(ContainerName, query, orgId?.ToString("D"), cancellationToken);
    }
}