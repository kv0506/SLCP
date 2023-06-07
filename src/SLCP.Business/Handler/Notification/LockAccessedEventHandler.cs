using MediatR;
using SLCP.Business.Notification;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Notification;

public class LockAccessedEventHandler : INotificationHandler<LockAccessedEvent>
{
	private readonly ILockAccessLogRepository _lockAccessLogRepository;

	public LockAccessedEventHandler(ILockAccessLogRepository lockAccessLogRepository)
	{
		_lockAccessLogRepository = lockAccessLogRepository;
	}

	public async Task Handle(LockAccessedEvent notification, CancellationToken cancellationToken)
	{
		await _lockAccessLogRepository.CreateItemAsync(new LockAccessLog
		{
			Id = Guid.NewGuid(),
			OrganizationId = notification.Lock.OrganizationId,
			Lock = notification.Lock,
			User = notification.User,
			AccessState = notification.AccessState,
			AccessDeniedReason = notification.AccessDeniedReason,
			AccessedDateTime = DateTimeOffset.UtcNow
		}, cancellationToken);
	}
}