using MediatR;
using Moq;
using Shouldly;
using SLCP.Business.Handler.Request;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.Business.Notification;
using SLCP.Core;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class ValidateLockAccessUsingAccessCodeCommandHandlerTests
{
	private IRequestHandler<ValidateLockAccessUsingAccessCodeCommand, LockAccessResponse> _handler;
	private Mock<IUserRepository> _userRepositoryMock;
	private Mock<ILockRepository> _lockRepositoryMock;
	private Mock<IMediator> _mediatorMock;
	private Mock<IRequestContext> _requestContextMock;

	private readonly Guid _orgId = Guid.NewGuid();
	private readonly Guid _userId = Guid.NewGuid();

	[SetUp]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IUserRepository>();
		_lockRepositoryMock = new Mock<ILockRepository>();
		_mediatorMock = new Mock<IMediator>();
		_requestContextMock = new Mock<IRequestContext>();

		_handler = new ValidateLockAccessUsingAccessCodeCommandHandler(_userRepositoryMock.Object,
			_lockRepositoryMock.Object, _mediatorMock.Object, _requestContextMock.Object);

		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(_orgId);
		_requestContextMock.SetupGet(x => x.UserId).Returns(_userId);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_ThrowsException_When_LockIsInvalid()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppException(ErrorCode.NotFound, string.Empty));

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User());

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.NotFound);

		_lockRepositoryMock.Verify(x=>x.GetByIdAsync(command.LockId,_orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x=>x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),Times.Never);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_ThrowsException_When_UserAccessCodeIsWrong()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new Lock());

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User { LockAccessCodeHash = "12345678" });

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var exception = await Should.ThrowAsync<AppException>(()=> _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.InvalidAccessCode);

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
				It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
				                                              lockAccessedEvent.AccessDeniedReason == AccessDeniedReason.InvalidUserLockAccessCode),
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_Returns_AccessDenied_When_UserDoesNotHaveAccess()
	{
		var lockObj = new Lock
		{
			Id = Guid.NewGuid(),
			IsEnabled = true,
			OrganizationId = _orgId
		};

		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => lockObj);

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User
			{
				LockAccessCodeHash = "123456",
				Tags = new List<Location>
				{
					new Location
					{
						Locks = new List<Lock>
						{
							new Lock { Id = Guid.NewGuid() }
						}
					}
				}
			});

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.DoesNotHaveAccessToLock);

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason == AccessDeniedReason.DoesNotHaveAccessToLock),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_Returns_AccessAllowed_When_UserHasAccess()
	{
		var lockObj = new Lock
		{
			Id = Guid.NewGuid(),
			IsEnabled = true,
			OrganizationId = _orgId
		};

		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => lockObj);

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User
			{
				LockAccessCodeHash = "123456", Tags = new List<Location>
				{
					new Location
					{
						Locks = new List<Lock>
						{
							lockObj
						}
					}
				}
			});

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeTrue();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Allowed),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}