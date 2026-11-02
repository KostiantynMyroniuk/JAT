namespace JAT.API.Common.Models
{
    public enum ErrorType { Failure, Validation, NotFound, Conflict, Unauthorized }
    public sealed record Error(string Description, ErrorType ErrorType, string? Code = null)
    {
        public static Error NotFound(string description, string? code = null) =>
            new(description, ErrorType.NotFound, code);

        public static Error Conflict(string description, string? code = null) =>
            new(description, ErrorType.Conflict, code);

        public static Error Validation(string description, string? propertyName = null) =>
            new(description, ErrorType.Validation, propertyName);

        public static Error Failure(string description, string? code = null) =>
            new(description, ErrorType.Failure, code);

        public static Error Unauthorized(string description, string? code = null) =>
            new(description, ErrorType.Unauthorized, code);
    };
}
