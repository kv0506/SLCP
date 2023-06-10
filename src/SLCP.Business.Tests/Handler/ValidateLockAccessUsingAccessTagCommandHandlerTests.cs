using MediatR;
using Moq;
using Shouldly;
using SLCP.Business.Handler.Request;
using SLCP.Business.Notification;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class ValidateLockAccessUsingAccessTagCommandHandlerTests
{
	private IRequestHandler<ValidateLockAccessUsingAccessTagCommand, LockAccessResponse> _handler;
	private Mock<IAccessTagRepository> _accessTagRepositoryMock;
	private Mock<IUserAccessGroupRepository> _userAccessGroupRepositoryMock;
	private Mock<IMediator> _mediatorMock;
	private Mock<IRequestContext> _requestContextMock;

	private readonly Guid _locationId = Guid.NewGuid();
	private readonly Guid _orgId = Guid.NewGuid();

	[SetUp]
	public void Setup()
	{
		_accessTagRepositoryMock = new Mock<IAccessTagRepository>();
		_userAccessGroupRepositoryMock = new Mock<IUserAccessGroupRepository>();
		_mediatorMock = new Mock<IMediator>();
		_requestContextMock = new Mock<IRequestContext>();

		_handler = new ValidateLockAccessUsingAccessTagCommandHandler(_userAccessGroupRepositoryMock.Object,
			_accessTagRepositoryMock.Object, _mediatorMock.Object, _requestContextMock.Object);

		_requestContextMock.SetupGet(x => x.Locations).Returns(new List<Guid> { _locationId });
		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(_orgId);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_ThrowsException_When_AccessTagIsInvalid()
	{
		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new List<UserAccessGroup>());

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppException(ErrorCode.NotFound, string.Empty));

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = Guid.NewGuid(),
			LocationId = Guid.NewGuid(),
			AccessTagId = Guid.NewGuid()
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.NotFound);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Never);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, command.LocationId, It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_Returns_AccessDenied_When_AccessTagIsBlocked()
	{
		_userAccessGroupRepositoryMock
			.Setup(x => x.GetByLockIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new List<UserAccessGroup>());

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag { IsBlocked = true });

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = Guid.NewGuid(),
			AccessTagId = Guid.NewGuid()
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.AccessTagIsBlocked);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Never);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, command.LocationId, It.IsAny<CancellationToken>()),
			Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason ==
			                                              AccessDeniedReason.AccessTagBlocked),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_Returns_AccessDenied_When_UserDoesNotHaveAccess()
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
						new User { Id = Guid.NewGuid() }
					}
				}
			});

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag
			{
				User = new User
				{
					Id = Guid.NewGuid()
				}
			});

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = lockObj.Id,
			LocationId = _locationId,
			AccessTagId = Guid.NewGuid()
		};

		var exception = await Should.ThrowAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
		exception.ErrorCode.ShouldBe(ErrorCode.DoesNotHaveAccessToLock);

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, command.LocationId, It.IsAny<CancellationToken>()),
			Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason ==
			                                              AccessDeniedReason.DoesNotHaveAccessToLock),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Never);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_Returns_AccessAllowed_When_UserHasAccess()
	{
		var lockObj = new Lock
		{
			Id = Guid.NewGuid(),
			IsEnabled = true,
			OrganizationId = _orgId
		};

		var userId = Guid.NewGuid();

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
						new User { Id = userId }
					}
				}
			});

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag
			{
				User = new User
				{
					Id = userId
				}
			});

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = lockObj.Id,
			LocationId = _locationId,
			AccessTagId = Guid.NewGuid()
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeTrue();

		_userAccessGroupRepositoryMock.Verify(
			x => x.GetByLockIdAsync(command.LockId, command.LocationId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, command.LocationId, It.IsAny<CancellationToken>()),
			Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Allowed),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}