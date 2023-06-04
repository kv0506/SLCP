﻿namespace SLCP.DataAccess.Contracts;

public interface ICosmosService
{
	Task<T> CreateItemAsync<T>(string containerName, T item, string? partitionKey, CancellationToken cancellationToken);
	Task<T> GetItemAsync<T>(string containerName, string itemId, string? partitionKey, CancellationToken cancellationToken);
	Task<IList<T>> GetItemsAsync<T>(string containerName, string query, string? partitionKey, CancellationToken cancellationToken);
	Task<QueryResult<T>> GetItemsAsync<T>(string containerName, string query, int pageSize, string? continuationToken, string? partitionKey, CancellationToken cancellationToken);

}