using Serilog.Context;

namespace PaymentGateway.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        httpContext.Response.OnStarting(() =>
        {
            httpContext.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await _next(httpContext);
        }
    }
}