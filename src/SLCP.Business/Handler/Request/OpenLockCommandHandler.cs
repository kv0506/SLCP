using MediatR;
using SLCP.Business.Request;
using SLCP.DataAccess.Repositories.Contracts;

namespace SLCP.Business.Handler.Request;

public class OpenLockCommandHandler : IRequestHandler<OpenLockCommand>
{
	private readonly ILockHubRepository _lockHubRepository;

	public OpenLockCommandHandler(ILockHubRepository lockHubRepository)
	{
		_lockHubRepository = lockHubRepository;
	}

	public async Task Handle(OpenLockCommand request, CancellationToken cancellationToken)
	{
		//var lockHub =
		//	await _lockHubRepository.GetByLockIdAsync(request.Lock.Id, request.Lock.LocationId, cancellationToken);

		//if (lockHub == null)
		//{
		//	// send the signal to lock hub to open the lock
		//}
	}
}