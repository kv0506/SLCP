using MediatR;
using Moq;
using Shouldly;
using SLCP.Business.Handler.Request;
using SLCP.Business.Notification;
using SLCP.Business.Request;
using SLCP.Business.Response;
using SLCP.Business.Services;
using SLCP.DataAccess.Exception;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class ValidateLockAccessUsingAccessTagCommandHandlerTests
{
	private IRequestHandler<ValidateLockAccessUsingAccessTagCommand, LockAccessResponse> _handler;
	private Mock<IAccessTagRepository> _accessTagRepositoryMock;
	private Mock<ILockRepository> _lockRepositoryMock;
	private Mock<IMediator> _mediatorMock;
	private Mock<IRequestContext> _requestContextMock;

	private readonly Guid _orgId = Guid.NewGuid();

	[SetUp]
	public void Setup()
	{
		_accessTagRepositoryMock = new Mock<IAccessTagRepository>();
		_lockRepositoryMock = new Mock<ILockRepository>();
		_mediatorMock = new Mock<IMediator>();
		_requestContextMock = new Mock<IRequestContext>();

		_handler = new ValidateLockAccessUsingAccessTagCommandHandler(_lockRepositoryMock.Object,
			_accessTagRepositoryMock.Object, _mediatorMock.Object, _requestContextMock.Object);

		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(_orgId);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_ThrowsException_When_AccessTagIsInvalid()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new Lock());

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppDomainException(string.Empty));

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = Guid.NewGuid(),
			AccessTagId = Guid.NewGuid()
		};

		await Should.ThrowAsync<AppDomainException>(() => _handler.Handle(command, CancellationToken.None));

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_Returns_AccessDenied_When_AccessTagIsBlocked()
	{
		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new Lock());

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag { IsBlocked = true });

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = Guid.NewGuid(),
			AccessTagId = Guid.NewGuid()
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeFalse();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason == AccessDeniedReason.AccessTagBlocked),
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_ValidateLockAccessUsingAccessTagCommand_Returns_AccessDenied_When_UserDoesNotHaveAccess()
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

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag { User = new User
			{
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
			}
			});

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = lockObj.Id,
			AccessTagId = Guid.NewGuid()
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeFalse();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Denied &&
			                                              lockAccessedEvent.AccessDeniedReason == AccessDeniedReason.DoesNotHaveAccessToLock),
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

		_lockRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => lockObj);

		_accessTagRepositoryMock
			.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new AccessTag
			{
				User = new User
				{
					PermittedLockGroups = new List<LockGroup>
					{
						new LockGroup
						{
							Locks = new List<Lock>
							{
								lockObj
							}
						}
					}
				}
			});

		var command = new ValidateLockAccessUsingAccessTagCommand
		{
			LockId = lockObj.Id,
			AccessTagId = Guid.NewGuid()
		};

		var response = await _handler.Handle(command, CancellationToken.None);
		response.AccessAllowed.ShouldBeTrue();

		_lockRepositoryMock.Verify(x => x.GetByIdAsync(command.LockId, _orgId, It.IsAny<CancellationToken>()), Times.Once);
		_accessTagRepositoryMock.Verify(x => x.GetByIdAsync(command.AccessTagId, _orgId, It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<LockAccessedEvent>(lockAccessedEvent => lockAccessedEvent.AccessState == AccessState.Allowed),
			It.IsAny<CancellationToken>()), Times.Once);

		_mediatorMock.Verify(x => x.Publish(
			It.Is<OpenLockEvent>(openLockEvent => openLockEvent.Lock.Id == lockObj.Id),
			It.IsAny<CancellationToken>()), Times.Once);
	}
}