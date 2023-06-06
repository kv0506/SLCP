using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface IUserRepository
{
    Task<User> GetByIdAsync(Guid id, Guid? orgId, CancellationToken cancellationToken);

    Task<User> GetByEmailAsync(string email, Guid? orgId, CancellationToken cancellationToken);
}