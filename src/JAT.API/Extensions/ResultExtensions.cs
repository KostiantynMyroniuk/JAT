using JAT.API.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace JAT.API.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToProblem(this Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("Can not convert successful result to a problem.");
            }

            var error = result.Error!;

            return error.ErrorType switch
            {
                ErrorType.NotFound => TypedResults.NotFound<ProblemDetails>(new()
                {
                    Detail = error.Description
                }),

                ErrorType.Conflict => TypedResults.Conflict<ProblemDetails>(new()
                {
                    Detail = error.Description
                }),

                ErrorType.Validation => TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    [error.Code ?? "General"] = [error.Description]
                }),

                _ => TypedResults.BadRequest<ProblemDetails>(new()
                {
                    Detail = error.Description
                })
            };
        }
    }
}
