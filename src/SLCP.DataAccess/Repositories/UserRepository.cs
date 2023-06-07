using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    public const string ContainerName = "users";

    private readonly ICosmosService _cosmosService;

    public UserRepository(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<User> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
    {
        return await _cosmosService.GetItemAsync<User>(ContainerName, id.ToHyphens(), orgId?.ToHyphens(),
            cancellationToken);
    }

    public async Task<User> GetByEmailAsync(string email, Guid? orgId, CancellationToken cancellationToken)
    {
	    var query = $"SELECT * FROM c WHERE c.emailAddress = {email}";
		var items = await _cosmosService.GetItemsAsync<User>(ContainerName, query, orgId?.ToHyphens(), cancellationToken);
		return items.SingleOrDefault();
    }
}