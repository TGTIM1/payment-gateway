using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Middleware;

    public class GlobalExceptionHandler : IExceptionHandler
    {
       private readonly ILogger<GlobalExceptionHandler> _logger;
       public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
                _logger.LogError(exception, "Произошла необработанная ошибка:{message}", exception.Message);
            var problemDetails = new ProblemDetails
            {
                Type = "https://httpstatuses.com/500",
                Title = "Server Error",
                Status = 500,
                Detail = "Произошла непредвиденная ошибка, пожалуйста повторите позже"
            };
            context.Response.StatusCode = problemDetails.Status.Value;
            
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
