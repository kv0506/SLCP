using FluentValidation;
using SLCP.Business.Request;

namespace SLCP.Business.Validator;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(x => x.Email).NotEmpty().Matches("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$");
		RuleFor(x=>x.Password).NotEmpty();
	}
}