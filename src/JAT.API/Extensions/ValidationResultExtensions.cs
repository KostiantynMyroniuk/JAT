using FluentValidation.Results;
using JAT.API.Common.Models;

namespace JAT.API.Extensions;

public static class ValidationResultExtensions
{
    public static Error ToValidationError(this ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Error.Validation(errors);
    }
}
