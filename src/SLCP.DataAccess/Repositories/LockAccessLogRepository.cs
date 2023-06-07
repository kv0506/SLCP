using System.Text;
using CSharpExtensions;
using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockAccessLogRepository : ILockAccessLogRepository
{
	private readonly ICosmosService _cosmosService;

	public LockAccessLogRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<LockAccessLog> CreateItemAsync(LockAccessLog accessLog, CancellationToken cancellationToken)
	{
		return await _cosmosService.CreateItemAsync(ContainerNames.LockAccessLogs, accessLog,
			accessLog.Lock.OrganizationId.ToHyphens(), cancellationToken);
	}

	public async Task<QueryResult<LockAccessLog>> GetItemsAsync(Guid? lockId, Guid? userId, Guid? orgId, int maxIemCount, string? continuationToken,
		CancellationToken cancellationToken)
	{
		var queryBuilder = new StringBuilder("SELECT * FROM c ");

		if (lockId.IsNotNullOrEmpty())
		{
			queryBuilder.Append($"WHERE c.lock.id = {lockId?.ToHyphens()}");
		}

		if (userId.IsNotNullOrEmpty())
		{
			queryBuilder.Append($" {(lockId.IsNotNullOrEmpty() ? "AND" : "WHERE")} c.user.id = {userId?.ToHyphens()}");
		}

		return await _cosmosService.GetItemsAsync<LockAccessLog>(ContainerNames.LockAccessLogs, queryBuilder.ToString(), maxIemCount, continuationToken, orgId?.ToHyphens(), cancellationToken);
	}
}