using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface IApiKeyRepository
{
	Task<ApiKey> GetByKeyAsync(string key, CancellationToken cancellationToken);
}