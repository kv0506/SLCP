using SLCP.DataAccess.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess;

public class LockGroupRepository : ILockGroupRepository
{
	private const string ContainerName = "lockgroups";

	private readonly ICosmosService _cosmosService;

	public LockGroupRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<IList<LockGroup>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken)
	{
		var query = "SELECT * FROM lockgroups lg WHERE ARRAY_CONTAINS(lg.locks, {\"id\": " + lockId.ToString("D") +
		            "})";
		return await _cosmosService.GetItemsAsync<LockGroup>(ContainerName, query, orgId?.ToString("D"), cancellationToken);
	}
}