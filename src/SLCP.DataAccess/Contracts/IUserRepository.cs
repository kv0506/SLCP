using SLCP.ServiceModel;

namespace SLCP.DataAccess.Contracts;

public interface IUserRepository
{
	Task<User> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken);
}