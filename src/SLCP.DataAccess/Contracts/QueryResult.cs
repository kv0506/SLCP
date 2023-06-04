namespace SLCP.DataAccess.Contracts;

public class QueryResult<T>
{
	public IList<T> Records { get; set; }

	public string ContinuationToken { get; set; }
}