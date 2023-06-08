using MediatR;
using SLCP.Business.Notification;
using SLCP.Business.Request;

namespace SLCP.Business.Handler.Notification;

public class LockAccessedEventHandler : INotificationHandler<LockAccessedEvent>
{
	private readonly IMediator _mediator;

	public LockAccessedEventHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task Handle(LockAccessedEvent notification, CancellationToken cancellationToken)
	{
		await _mediator.Send(new CreateLockAccessLogCommand
		{
			User = notification.User,
			Lock = notification.Lock,
			AccessState = notification.AccessState,
			AccessDeniedReason = notification.AccessDeniedReason
		}, cancellationToken);
	}
}