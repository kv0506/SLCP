using SLCP.ServiceModel;

namespace SLCP.DataAccess.Contracts;

public interface ILockGroupRepository
{
	Task<IList<LockGroup>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken);
}