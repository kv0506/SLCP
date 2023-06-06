namespace SLCP.API.Model;

public class ApiResponse
{
	public bool IsSuccess { get; set; }

	public object Result { get; set; }

	public IEnumerable<string> Errors { get; set; }
}