using FluentValidation;

namespace JAT.API.Filters
{
    public sealed class ValidationFilter<TRequest> : IEndpointFilter where TRequest : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();

            if (validator == null)
                return await next(context);

            var argument = context.Arguments.OfType<TRequest>().FirstOrDefault();

            if (argument == null)
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Request body is required");

            var result = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);

            if (!result.IsValid)
                return Results.ValidationProblem(result.ToDictionary());

            return await next(context);
        }
    }

    public static class ValidationExtension
    {
        public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
        {
            return builder.AddEndpointFilter<ValidationFilter<T>>()
                .ProducesValidationProblem();
        }
    }
}
