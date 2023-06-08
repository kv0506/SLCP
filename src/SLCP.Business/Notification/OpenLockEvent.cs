using MediatR;
using SLCP.ServiceModel;

namespace SLCP.Business.Notification;

public class OpenLockEvent : INotification
{
	public OpenLockEvent(Lock lockObj)
	{
		Lock = lockObj;
	}

	public Lock Lock { get; }
}