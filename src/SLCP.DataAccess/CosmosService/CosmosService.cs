using CSharpExtensions;
using Microsoft.Azure.Cosmos;
using SLCP.Core;

namespace SLCP.DataAccess.CosmosService;

public class CosmosService : ICosmosService
{
	private readonly CosmosClient _cosmosClient;
	private readonly Database _database;

	public CosmosService(CosmosSettings cosmosSettings)
	{
		ArgumentNullException.ThrowIfNull(cosmosSettings);

		_cosmosClient = new CosmosClient(cosmosSettings.ConnectionString, new CosmosClientOptions
		{
			SerializerOptions = new CosmosSerializationOptions
				{ PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase },
		});
		_database = _cosmosClient.GetDatabase(cosmosSettings.DatabaseName);
	}

	public async Task CreateContainerIfNotExistsAsync(string containerName, string partitionKeyPath,
		CancellationToken cancellationToken)
	{
		try
		{
			await _database.CreateContainerIfNotExistsAsync(containerName, partitionKeyPath,
				cancellationToken: cancellationToken);
		}
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.DatabaseError, "Container creation failed", ex);
		}
	}

	public async Task<T> CreateItemAsync<T>(string containerName, T item, string? partitionKey,
		CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
			}

			var response = await container.CreateItemAsync(item: item, partitionKey: GetPartitionKey(partitionKey),
				cancellationToken: cancellationToken);

			return response.Resource;
		}
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.DatabaseError, "Resource creation failed", ex);
		}
	}

	public async Task<T> GetItemAsync<T>(string containerName, string itemId, string? partitionKey,
		CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
			}

			return await container.ReadItemAsync<T>(id: itemId, partitionKey: GetPartitionKey(partitionKey),
				cancellationToken: cancellationToken);
		}
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.NotFound, $"Resource [Id={itemId}] not found", ex);
		}
	}

	public async Task<T> GetItemAsync<T>(string containerName, string query, CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
			}

			var response = container.GetItemQueryIterator<T>(query);
			if (response.HasMoreResults)
			{
				var itemResponse = await response.ReadNextAsync(cancellationToken);
				return itemResponse.Resource.FirstOrDefault();
			}
		}
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.NotFound, "Resource not found", ex);
		}

		return default;
	}

	public async Task<IList<T>> GetItemsAsync<T>(string containerName, string query, string? partitionKey,
		CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
			}

			var queryDefinition = new QueryDefinition(
				query: query
			);

			using var queryResultSetIterator = container.GetItemQueryIterator<T>(
				queryDefinition: queryDefinition, requestOptions: new QueryRequestOptions
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
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.NotFound, "Resources not found", ex);
		}
	}

	public async Task<QueryResult<T>> GetItemsAsync<T>(string containerName, string query, int pageSize,
		string? continuationToken,
		string? partitionKey, CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
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
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.NotFound, "Resources not found", ex);
		}
	}

	public async Task<T> UpsertItemAsync<T>(string containerName, T item, string? partitionKey,
		CancellationToken cancellationToken)
	{
		try
		{
			var container = GetContainer(containerName);
			if (container == null)
			{
				throw new AppException(ErrorCode.DatabaseError, $"Container does not exist {containerName}");
			}

			return (await container.UpsertItemAsync(item: item, partitionKey: GetPartitionKey(partitionKey),
				cancellationToken: cancellationToken)).Resource;
		}
		catch (CosmosException ex)
		{
			throw new AppException(ErrorCode.DatabaseError, "Resource insertion failed", ex);
		}
	}

	private Container GetContainer(string containerName)
	{
		ArgumentNullException.ThrowIfNull(containerName);
		return _database.GetContainer(containerName);
	}

	private PartitionKey GetPartitionKey(string? partitionKey)
	{
		return partitionKey.IsNullOrEmpty() ? PartitionKey.Null : new PartitionKey(partitionKey);
	}
}