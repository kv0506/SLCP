using MediatR;
using Moq;
using SLCP.Business.Handler.Request;
using SLCP.Business.Request;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class CreateLockAccessLogCommandHandlerTests
{
	private IRequestHandler<CreateLockAccessLogCommand> _handler;
	private Mock<ILockAccessLogRepository> _lockAccessLogRepositoryMock;

	[SetUp]
	public void Setup()
	{
		_lockAccessLogRepositoryMock = new Mock<ILockAccessLogRepository>();
		_handler = new CreateLockAccessLogCommandHandler(_lockAccessLogRepositoryMock.Object);

		_lockAccessLogRepositoryMock
			.Setup(x => x.CreateItemAsync(It.IsAny<LockAccessLog>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new LockAccessLog());
	}

	[Test]
	public async Task Handle_CreateLockAccessLogCommand_NewItemIsCreated()
	{
		var command = new CreateLockAccessLogCommand
		{
			Lock = new Lock(),
			User = new User(),
			AccessState = AccessState.Allowed
		};

		await _handler.Handle(command, CancellationToken.None);

		_lockAccessLogRepositoryMock.Verify(
			x => x.CreateItemAsync(It.IsAny<LockAccessLog>(), It.IsAny<CancellationToken>()), Times.Once);
	}
}