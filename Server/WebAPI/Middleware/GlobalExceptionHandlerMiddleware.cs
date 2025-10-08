namespace WebAPI;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(
                $"Error occure while processing the request, TraceId : ${traceId}," +
                $" Message : ${ex.Message}, StackTrace: ${ex.StackTrace}");
            context.Response.StatusCode = 404; //Not Found
            await context.Response.WriteAsync(ex.Message);
        }
        catch (ArgumentException ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(
                $"Error occure while processing the request, TraceId : ${traceId}," +
                $" Message : ${ex.Message}, StackTrace: ${ex.StackTrace}");
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsync(ex.Message);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(
                $"Error occure while processing the request, TraceId : ${traceId}," +
                $" Message : ${ex.Message}, StackTrace: ${ex.StackTrace}");
            context.Response.StatusCode = 500; // Internal Server Error
            await context.Response.WriteAsync(ex.Message);
        }
    }
}