using MediatR;
using SLCP.Business.Request;

namespace SLCP.Business.Handler.Request;

public class OpenLockCommandHandler : IRequestHandler<OpenLockCommand>
{
	public async Task Handle(OpenLockCommand request, CancellationToken cancellationToken)
	{
		// TODO
		// Send signal to the lock to unlock it
	}
}