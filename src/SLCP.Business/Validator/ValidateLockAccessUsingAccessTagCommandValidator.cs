using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class ValidateLockAccessUsingAccessTagCommandValidator : AbstractValidator<ValidateLockAccessUsingAccessTagCommand>
{
	public ValidateLockAccessUsingAccessTagCommandValidator()
	{
		RuleFor(x => x.LocationId).NotEmpty();
		RuleFor(x => x.LockId).NotEmpty();
		RuleFor(x => x.AccessTagId).NotEmpty();
	}
}