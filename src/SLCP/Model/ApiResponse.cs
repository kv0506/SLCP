using SLCP.Core;

namespace SLCP.API.Model;

public class ApiResponse
{
	public bool IsSuccess { get; set; }

	public object Result { get; set; }

	public IEnumerable<Error> Errors { get; set; }
}


public class Error
{
	public Error(string code, string message)
	{
		Code = code;
		Message = message;
	}

	public Error(ErrorCode code, string message)
	{
		Code = code.ToString();
		Message = message;
	}

	public string Message { get; }
	public string Code { get; }
}