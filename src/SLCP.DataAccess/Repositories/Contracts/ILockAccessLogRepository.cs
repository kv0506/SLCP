using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockAccessLogRepository
{
	Task<LockAccessLog> CreateItemAsync(LockAccessLog accessLog, CancellationToken cancellationToken);

	Task<QueryResult<LockAccessLog>> GetItemsAsync(Guid? lockId, Guid? userId, Guid? orgId, int maxIemCount,
		string? continuationToken, CancellationToken cancellationToken);
}