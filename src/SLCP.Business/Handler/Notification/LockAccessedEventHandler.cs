using MediatR;
using SLCP.Business.Notification;

namespace SLCP.Business.Handler.Notification;

public class LockAccessedEventHandler : INotificationHandler<LockAccessedEvent>
{
	public async Task Handle(LockAccessedEvent notification, CancellationToken cancellationToken)
	{
		
	}
}