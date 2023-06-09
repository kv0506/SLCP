using SLCP.Core;
using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class UserAccessGroupRepository : IUserAccessGroupRepository
{
	private readonly ICosmosService _cosmosService;

	public UserAccessGroupRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<IList<UserAccessGroup>> GetByLockIdAsync(Guid lockId, Guid locationId, CancellationToken cancellationToken)
	{
		var query = "SELECT * FROM c WHERE ARRAY_CONTAINS(c.locks, {id: '" + lockId.ToHyphens() +
		            "'}, true)";
		return await _cosmosService.GetItemsAsync<UserAccessGroup>(ContainerNames.UserAccessGroups, query, locationId.ToHyphens(), cancellationToken);
	}
}