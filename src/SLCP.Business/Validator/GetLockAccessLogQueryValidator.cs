using CSharpExtensions;
using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class GetLockAccessLogQueryValidator : AbstractValidator<GetLockAccessLogQuery>
{
	public GetLockAccessLogQueryValidator()
	{
		RuleFor(x => x.LockId).NotNull().NotEmpty().When(x => x.UserId.IsNullOrEmpty());
		RuleFor(x => x.UserId).NotNull().NotEmpty().When(x => x.LockId.IsNullOrEmpty());
	}
}