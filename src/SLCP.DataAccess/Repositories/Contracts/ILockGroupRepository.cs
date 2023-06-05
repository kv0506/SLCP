using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockGroupRepository
{
	Task<LockGroup> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken);
	Task<IList<LockGroup>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken);
}