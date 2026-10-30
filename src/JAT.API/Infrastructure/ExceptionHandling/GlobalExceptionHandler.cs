using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JAT.API.Infrastructure.ExceptionHandling
{
    public class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Internal server error");
            else
                logger.LogWarning("Handled exception: {Message}", exception.Message);

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext() 
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Instance = httpContext.Request.Path
                }
            });
        }
    }
}
