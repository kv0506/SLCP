using MediatR;
using Moq;
using SLCP.Business.Handler.Notification;
using SLCP.Business.Notification;
using SLCP.Business.Request;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class LockAccessedEventHandlerTests
{
	private INotificationHandler<LockAccessedEvent> _handler;
	private Mock<IMediator> _mediatorMock;

	[SetUp]
	public void Setup()
	{
		_mediatorMock = new Mock<IMediator>();
		_handler = new LockAccessedEventHandler(_mediatorMock.Object);
	}

	[Test]
	public async Task Handle_LockAccessedEvent_Sends_CreateLockAccessLogCommand()
	{
		await _handler.Handle(new LockAccessedEvent(new Lock(), new User(), AccessState.Allowed, null), CancellationToken.None);

		_mediatorMock.Verify(x => x.Send(It.IsAny<CreateLockAccessLogCommand>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}
}