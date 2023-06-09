﻿using SLCP.ServiceModel;

namespace SLCP.DataAccess.Repositories.Contracts;

public interface IAccessTagRepository
{
	Task<AccessTag> GetByIdAsync(Guid id, Guid locationId, CancellationToken cancellationToken);
}