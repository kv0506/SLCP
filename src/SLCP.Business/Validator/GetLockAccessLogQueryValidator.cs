using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class GetLockAccessLogQueryValidator : AbstractValidator<GetLockAccessLogQuery>
{
	public GetLockAccessLogQueryValidator()
	{
		RuleFor(x => x.LocationId).NotEmpty();
	}
}