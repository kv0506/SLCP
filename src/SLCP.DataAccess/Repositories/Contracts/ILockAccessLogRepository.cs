using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockAccessLogRepository
{
	Task<LockAccessLog> CreateItemAsync(LockAccessLog accessLog, CancellationToken cancellationToken);

	Task<QueryResult<LockAccessLog>> GetItemsAsync(Guid? lockId, Guid? userId, Guid locationId, int maxIemCount,
		string? continuationToken, CancellationToken cancellationToken);
}