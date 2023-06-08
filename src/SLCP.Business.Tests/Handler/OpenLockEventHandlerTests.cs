using MediatR;
using Moq;
using SLCP.Business.Handler.Notification;
using SLCP.Business.Notification;
using SLCP.Business.Request;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class OpenLockEventHandlerTests
{
	private INotificationHandler<OpenLockEvent> _handler;
	private Mock<IMediator> _mediatorMock;

	[SetUp]
	public void Setup()
	{
		_mediatorMock = new Mock<IMediator>();
		_handler = new OpenLockEventHandler(_mediatorMock.Object);
	}

	[Test]
	public async Task Handle_LockAccessedEvent_Sends_CreateLockAccessLogCommand()
	{
		await _handler.Handle(new OpenLockEvent(new Lock()), CancellationToken.None);

		_mediatorMock.Verify(x => x.Send(It.IsAny<OpenLockCommand>(), It.IsAny<CancellationToken>()),
			Times.Once);
	}
}