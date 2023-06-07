using MediatR;
using SLCP.Business.Request;
using SLCP.DataAccess.Repositories.Contracts;
using SLCP.ServiceModel;

namespace SLCP.Business.Handler.Request;

public class CreateLockAccessLogCommandHandler : IRequestHandler<CreateLockAccessLogCommand>
{
	private readonly ILockAccessLogRepository _lockAccessLogRepository;

	public CreateLockAccessLogCommandHandler(ILockAccessLogRepository lockAccessLogRepository)
	{
		_lockAccessLogRepository = lockAccessLogRepository;
	}

	public async Task Handle(CreateLockAccessLogCommand request, CancellationToken cancellationToken)
	{
		var lockAccessLog = new LockAccessLog
		{
			Id = Guid.NewGuid(),
			Lock = request.Lock,
			User = request.User,
			AccessState = request.AccessState,
			AccessDeniedReason = request.AccessDeniedReason,
			AccessedDateTime = DateTimeOffset.Now,
			OrganizationId = request.Lock.OrganizationId
		};

		await _lockAccessLogRepository.CreateItemAsync(lockAccessLog, cancellationToken);
	}
}