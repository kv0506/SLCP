﻿using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class ValidateLockAccessUsingAccessCodeCommandValidator : AbstractValidator<ValidateLockAccessUsingAccessCodeCommand>
{
	public ValidateLockAccessUsingAccessCodeCommandValidator()
	{
		RuleFor(x => x.LocationId).NotEmpty();
		RuleFor(x => x.LockId).NotEmpty();
		RuleFor(x => x.UserLockAccessCode).NotEmpty();
	}
}