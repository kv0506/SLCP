using MediatR;
using Moq;
using Shouldly;
using SLCP.Business.Handler.Request;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Exception;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.Business.Notification;
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
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_ThrowsException_When_LockIsInvalid()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppDomainException(string.Empty));

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User());

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		await Should.ThrowAsync<AppDomainException>(() => _handler.Handle(command, CancellationToken.None));

		_lockRepositoryMock.Verify(x=>x.GetByIdAsync(command.LockId,_orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x=>x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),Times.Never);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_Returns_AccessDenied_When_UserAccessCodeIsWrong()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new Lock());

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User { LockAccessCode = "12345678" });

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeFalse();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(command.UserId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

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
				LockAccessCode = "123456",
				PermittedLockGroups = new List<LockGroup>
				{
					new LockGroup
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
			UserId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeFalse();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(command.UserId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

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
				LockAccessCode = "123456", PermittedLockGroups = new List<LockGroup>
				{
					new LockGroup
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
			UserId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeTrue();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(command.UserId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Allowed),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}