using CSharpExtensions;
using Microsoft.Azure.Cosmos;
using SLCP.DataAccess.Contracts;

namespace SLCP.DataAccess;

public class CosmosService : ICosmosService
{
	private readonly CosmosClient _cosmosClient;
	private readonly Database _database;

	public CosmosService(string connectionString, string databaseName)
	{
		ArgumentNullException.ThrowIfNull(connectionString);
		ArgumentNullException.ThrowIfNull(databaseName);

		_cosmosClient = new CosmosClient(connectionString);
		_database = _cosmosClient.GetDatabase(databaseName);
	}

	public async Task<T> CreateItemAsync<T>(string containerName, T item, string? partitionKey, CancellationToken cancellationToken)
	{
		var container = GetContainer(containerName);
		if (container == null)
		{
			throw new Exception($"Container does not exist {containerName}");
		}

		return await container.CreateItemAsync<T>(item: item, partitionKey: GetPartitionKey(partitionKey),
			cancellationToken: cancellationToken);
	}

	public async Task<T> GetItemAsync<T>(string containerName, string itemId, string? partitionKey, CancellationToken cancellationToken)
	{
		var container = GetContainer(containerName);
		if (container == null)
		{
			throw new Exception($"Container does not exist {containerName}");
		}

		return await container.ReadItemAsync<T>(id: itemId, partitionKey: GetPartitionKey(partitionKey),
			cancellationToken: cancellationToken);
	}

	public async Task<IList<T>> GetItemsAsync<T>(string containerName, string query, string? partitionKey, CancellationToken cancellationToken)
	{
		var container = GetContainer(containerName);
		if (container == null)
		{
			throw new Exception($"Container does not exist {containerName}");
		}

		var queryDefinition = new QueryDefinition(
			query: query
		);

		using var queryResultSetIterator = container.GetItemQueryIterator<T>(
			queryDefinition: queryDefinition, requestOptions:new QueryRequestOptions
			{
				PartitionKey = GetPartitionKey(partitionKey)
			}
		);

		var records = new List<T>();

		while (queryResultSetIterator.HasMoreResults)
		{
			var response = await queryResultSetIterator.ReadNextAsync(cancellationToken);
			records.AddRange(response);
		}

		return records;
	}

	public async Task<QueryResult<T>> GetItemsAsync<T>(string containerName, string query, int pageSize, string? continuationToken, 
		string? partitionKey, CancellationToken cancellationToken)
	{
		var container = GetContainer(containerName);
		if (container == null)
		{
			throw new Exception($"Container does not exist {containerName}");
		}

		var queryDefinition = new QueryDefinition(
			query: query
		);

		using var queryResultSetIterator = container.GetItemQueryIterator<T>(
			queryDefinition: queryDefinition, requestOptions: new QueryRequestOptions
			{
				MaxItemCount = pageSize,
				PartitionKey = GetPartitionKey(partitionKey)
			},
			continuationToken: continuationToken
		);

		var queryResult = new QueryResult<T>();

		var response = await queryResultSetIterator.ReadNextAsync(cancellationToken);
		queryResult.Records = response.ToList();
		queryResult.ContinuationToken = response.ContinuationToken;

		return queryResult;
	}

	private Container GetContainer(string containerName)
	{
		ArgumentNullException.ThrowIfNull(containerName);
		return _database.GetContainer(containerName);
	}

	private PartitionKey GetPartitionKey(string? partitionKey)
	{
		return partitionKey.IsNullOrEmpty() ? PartitionKey.None : new PartitionKey(partitionKey);
	}
}