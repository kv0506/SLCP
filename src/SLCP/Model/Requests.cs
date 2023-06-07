using System.ComponentModel.DataAnnotations;

namespace SLCP.API.Model;

public class Login
{
	[Required]
	[RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", ErrorMessage = "Invalid email address")]
	public string Email { get; set; }

	[Required]
	public string Password { get; set; }
}