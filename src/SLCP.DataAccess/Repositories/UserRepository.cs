﻿using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private const string ContainerName = "users";

    private readonly ICosmosService _cosmosService;

    public UserRepository(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<User> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken)
    {
        return await _cosmosService.GetItemAsync<User>(ContainerName, id.ToString("D"), orgId?.ToString("D"),
            cancellationToken);
    }
}