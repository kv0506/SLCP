using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockRepository
{
	Task<Lock> GetByIdAsync(Guid id, Guid locationId, CancellationToken cancellationToken);
}