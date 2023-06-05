using System.Text;
using CSharpExtensions;
using SLCP.DataAccess.CosmosService;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories;

public class LockAccessLogRepository : ILockAccessLogRepository
{
	private const string ContainerName = "lock-access-logs";

	private readonly ICosmosService _cosmosService;

	public LockAccessLogRepository(ICosmosService cosmosService)
	{
		_cosmosService = cosmosService;
	}

	public async Task<LockAccessLog> CreateItemAsync(LockAccessLog accessLog, CancellationToken cancellationToken)
	{
		return await _cosmosService.CreateItemAsync(ContainerName, accessLog,
			accessLog.Lock.Organization.Id.ToString("D"), cancellationToken);
	}

	public async Task<QueryResult<LockAccessLog>> GetItemsAsync(Guid? lockId, Guid? userId, Guid? orgId, int maxIemCount, string? continuationToken,
		CancellationToken cancellationToken)
	{
		var queryBuilder = new StringBuilder("SELECT * FROM lock-access-logs al WHERE");

		if (lockId.IsNotNullOrEmpty())
		{
			queryBuilder.Append($" al.lock.id = {lockId}");
		}

		if (userId.IsNotNullOrEmpty())
		{
			queryBuilder.Append($" {(lockId.IsNotNullOrEmpty() ? "AND" : "")} al.user.id = {userId}");
		}

		return await _cosmosService.GetItemsAsync<LockAccessLog>(ContainerName, queryBuilder.ToString(), maxIemCount, continuationToken, orgId?.ToString("D"), cancellationToken);
	}
}