using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILocationRepository
{
	Task<Location> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken);
	Task<IList<Location>> GetByLockIdAsync(Guid lockId, Guid? orgId, CancellationToken cancellationToken);
}