using SLCP.Core;
using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ICosmosService _cosmosService;

    public LocationRepository(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<Location> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
    {
		return await _cosmosService.GetItemAsync<Location>(ContainerNames.Locations, id.ToHyphens(), orgId?.ToHyphens(),
			cancellationToken);
	}

    public async Task<IList<Location>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.locks, {id: '" + lockId.ToHyphens() +
                    "'}, true)";
        return await _cosmosService.GetItemsAsync<Location>(ContainerNames.Locations, query, orgId?.ToHyphens(), cancellationToken);
    }
}