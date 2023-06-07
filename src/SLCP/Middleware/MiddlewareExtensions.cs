namespace SLCP.API.Middleware;

public static class MiddlewareExtensions
{
	public static void UseAppExceptionHandler(this IApplicationBuilder app)
	{
		app.UseMiddleware<AppExceptionHandlerMiddleware>();
	}
}