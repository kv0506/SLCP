using CSharpExtensions;
using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class GetLockAccessLogQueryValidator : AbstractValidator<GetLockAccessLogQuery>
{
	public GetLockAccessLogQueryValidator()
	{
		RuleFor(x => x.LockId).NotEmpty().When(x => x.UserId.IsNullOrEmpty());
		RuleFor(x => x.UserId).NotEmpty().When(x => x.LockId.IsNullOrEmpty());
	}
}