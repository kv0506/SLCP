using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Notification;

public class LockAccessedEvent : INotification
{
	public LockAccessedEvent(Lock lockObj, User? user, AccessState accessState,
		AccessDeniedReason? accessDeniedReason)
	{
		Lock = lockObj;
		User = user;
		AccessState = accessState;
		AccessDeniedReason = accessDeniedReason;
	}

	public Lock Lock { get; }

	public User? User { get; }

	public AccessState AccessState { get; }

	public AccessDeniedReason? AccessDeniedReason { get; }
}