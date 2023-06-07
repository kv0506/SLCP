using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockRepository : ILockRepository
{
	private const string ContainerName = "locks";

	private readonly ICosmosService _cosmosService;

	public LockRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<Lock> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
	{
		return await _cosmosService.GetItemAsync<Lock>(ContainerName, id.ToHyphens(), orgId?.ToHyphens(),
			cancellationToken);
	}

	public async Task<IList<Lock>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken)
	{
		var query = "SELECT * FROM c";
		return await _cosmosService.GetItemsAsync<Lock>(ContainerName, query, orgId.ToHyphens(), cancellationToken);
	}
}