using MediatR;
using SLCP.Business.Notification;
using SLCP.Business.Request;

namespace SLCP.Business.Handler.Notification;

public class OpenLockEventHandler : INotificationHandler<OpenLockEvent>
{
	private readonly IMediator _mediator;

	public OpenLockEventHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task Handle(OpenLockEvent notification, CancellationToken cancellationToken)
	{
		await _mediator.Send(new OpenLockCommand { Lock = notification.Lock }, cancellationToken);
	}
}