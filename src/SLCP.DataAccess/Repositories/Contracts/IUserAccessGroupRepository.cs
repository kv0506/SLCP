using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface IUserAccessGroupRepository
{
	Task<IList<UserAccessGroup>> GetByLockIdAsync(Guid lockId, Guid locationId, CancellationToken cancellationToken);
}