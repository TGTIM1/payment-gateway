using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Exceptions;

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
            var (statusCode, errorMessage) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
                ConflictException => (StatusCodes.Status409Conflict, "Resource Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = errorMessage,
                Type = exception.GetType().Name,
                Detail = exception.Message
            };
            
            context.Response.StatusCode = statusCode;
            
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
