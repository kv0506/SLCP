using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface ILockRepository
{
	Task<Lock> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken);

	Task<IList<Lock>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken);
}