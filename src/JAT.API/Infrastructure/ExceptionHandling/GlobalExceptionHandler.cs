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
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("Request cancelled by user. {Method} {Path}. TraceId: {TraceId}",
                    httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);

                return true;
            }

            var (statusCode, title) = exception switch
            {
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Unhandled exception. {Method} {Path} -> {StatusCode}. TraceId: {TraceId}",
                    httpContext.Request.Method, httpContext.Request.Path, statusCode, httpContext.TraceIdentifier);
            else
                logger.LogWarning(exception, "Handled exception {ExceptionType}: {Message}. {Method} {Path} -> {StatusCode}. TraceId: {TraceId}", 
                    exception.GetType().Name, exception.Message, httpContext.Request.Method, httpContext.Request.Path, statusCode, httpContext.TraceIdentifier);

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext() 
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Instance = httpContext.Request.Path,
                    Extensions = { ["traceId"] = httpContext.TraceIdentifier }
                }
            });
        }
    }
}
