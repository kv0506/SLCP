using MediatR;
using Moq;
using Shouldly;
using SLCP.Business.Handler.Request;
using SLCP.Business.Request;
using SLCP.Business.Services;
using SLCP.Core;
using SLCP.DataAccess;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Tests.Handler;

[TestFixture]
public class GetLockAccessLogQueryHandlerTests
{
	private IRequestHandler<GetLockAccessLogQuery, QueryResult<LockAccessLog>> _handler;
	private Mock<ILockAccessLogRepository> _lockAccessLogRepositoryMock;
	private Mock<IRequestContext> _requestContextMock;

	[SetUp]
	public void Setup()
	{
		_lockAccessLogRepositoryMock = new Mock<ILockAccessLogRepository>();
		_requestContextMock = new Mock<IRequestContext>();
		_handler = new GetLockAccessLogQueryHandler(_lockAccessLogRepositoryMock.Object, _requestContextMock.Object);
	}

	[Test]
	public async Task Handle_GetLockAccessLogsQuery_ThrowException_When_OrganizationIdIsNull()
	{
		_lockAccessLogRepositoryMock
			.Setup(x => x.GetItemsAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int>(),
				It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new AppException(ErrorCode.NotFound, string.Empty));

		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(Guid.Empty);

		var query = new GetLockAccessLogQuery
		{
			LockId = Guid.NewGuid()
		};

		await Should.ThrowAsync<AppException>(() => _handler.Handle(query, CancellationToken.None));

		_lockAccessLogRepositoryMock.Verify(
			x => x.GetItemsAsync(query.LockId, query.UserId, Guid.Empty, query.PageSize, query.ContinuationToken,
				It.IsAny<CancellationToken>()), Times.Once);
	}

	[Test]
	public async Task Handle_GetLockAccessLogsQuery_ReturnLogs_When_OnlyLockIdIsProvided()
	{
		var lockId = Guid.NewGuid();
		var orgId = Guid.NewGuid();

		_lockAccessLogRepositoryMock
			.Setup(x => x.GetItemsAsync(It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int>(),
				It.IsAny<string?>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => new QueryResult<LockAccessLog>()
			{
				Records = new List<LockAccessLog>
				{
					new LockAccessLog
					{
						Lock = new Lock { Id = lockId },
						User = new User { Id = Guid.NewGuid() }
					},
					new LockAccessLog
					{
						Lock = new Lock { Id = lockId },
						User = new User { Id = Guid.NewGuid() }
					},
				}
			});

		var query = new GetLockAccessLogQuery
		{
			LockId = lockId
		};

		_requestContextMock.SetupGet(x => x.OrganizationId).Returns(orgId);

		var queryResult = await _handler.Handle(query, CancellationToken.None);

		queryResult.Records.ShouldAllBe(x => x.Lock.Id == lockId);

		_lockAccessLogRepositoryMock.Verify(
			x => x.GetItemsAsync(query.LockId, query.UserId, orgId, query.PageSize, query.ContinuationToken,
				It.IsAny<CancellationToken>()), Times.Once);
	}
}