using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockHubRepository
{
	Task<LockHub> GetByLockIdAsync(Guid lockId, Guid locationId, CancellationToken cancellationToken);
}