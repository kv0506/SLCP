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
	private Mock<IUserAccessGroupRepository> _userAccessGroupRepositoryMock;
	private Mock<IMediator> _mediatorMock;
	private Mock<IRequestContext> _requestContextMock;

	private readonly Guid _orgId = Guid.NewGuid();
	private readonly Guid _locationId = Guid.NewGuid();
	private readonly Guid _userId = Guid.NewGuid();

	[SetUp]
	public void Setup()
	{
		_userRepositoryMock = new Mock<IUserRepository>();
		_userAccessGroupRepositoryMock = new Mock<IUserAccessGroupRepository>();
		_mediatorMock = new Mock<IMediator>();
		_requestContextMock = new Mock<IRequestContext>();

		_handler = new ValidateLockAccessUsingAccessCodeCommandHandler(_userRepositoryMock.Object,
			_userAccessGroupRepositoryMock.Object, _mediatorMock.Object, _requestContextMock.Object);

		_requestContextMock.SetupGet(x => x.UserId).Returns(_userId);
		_requestContextMock.SetupGet(x => x.Locations).Returns(new List<Guid>() { _locationId });
		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(_orgId);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_ThrowsException_When_LockIsInvalid()
	{
		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppException(ErrorCode.NotFound, string.Empty));

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User{LockAccessCodeHash = "uF42wQprBRJdWoacjU+M3+bBSwV+KtgpH8fJPQJwyDg=", Salt = "5691519521109491810" });

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			LocationId = Guid.NewGuid(),
			UserLockAccessCode = "12345678"
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.NotFound);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(
			x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_ThrowsException_When_UserAccessCodeIsWrong()
	{
		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new List<UserAccessGroup>());

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User { LockAccessCodeHash = "uF42wQprBRJdWoacjU+M3+bBSwV+KtgpH8fJPQJwyDg=", Salt = "5691519521109491810" });

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = Guid.NewGuid(),
			LocationId = Guid.NewGuid(),
			UserLockAccessCode = "123456"
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.InvalidAccessCode);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Never);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()),
			Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason ==
			                                              AccessDeniedReason.InvalidUserLockAccessCode),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_Returns_AccessDenied_When_UserDoesNotHaveAccess()
	{
		var lockId = Guid.NewGuid();

		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new List<UserAccessGroup>
			{
				new UserAccessGroup
				{
					Locks = new List<Lock>
					{
						new Lock { Id = lockId }
					},
					Users = new List<User>
					{
						new User { Id = Guid.NewGuid() }
					}
				}
			});

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User { LockAccessCodeHash = "uF42wQprBRJdWoacjU+M3+bBSwV+KtgpH8fJPQJwyDg=", Salt = "5691519521109491810" });

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = lockId,
			LocationId = Guid.NewGuid(),
			UserLockAccessCode = "12345678"
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.DoesNotHaveAccessToLock);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason ==
			                                              AccessDeniedReason.DoesNotHaveAccessToLock),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == command.LockId),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessCodeCommand_Returns_AccessAllowed_When_UserHasAccess()
	{
		var lockObj = new Lock
		{
			Id = Guid.NewGuid(),
			IsEnabled = true,
			LocationId = _locationId,
			OrganizationId = _orgId
		};

		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new List<UserAccessGroup>
			{
				new UserAccessGroup
				{
					Locks = new List<Lock>
					{
						lockObj
					},
					Users = new List<User>
					{
						new User { Id = _userId }
					}
				}
			});

		_userRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new User
			{
				Id = _userId,
				LockAccessCodeHash = "uF42wQprBRJdWoacjU+M3+bBSwV+KtgpH8fJPQJwyDg=",
				Salt = "5691519521109491810"
			});

		var command = new ValidateLockAccessUsingAccessCodeCommand
		{
			LockId = lockObj.Id,
			UserLockAccessCode = "12345678"
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeTrue();

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Once);
		_userRepositoryMock.Verify(x => x.GetByIdAsync(_userId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Allowed),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}