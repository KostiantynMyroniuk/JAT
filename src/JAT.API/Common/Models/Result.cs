namespace JAT.API.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error != null)
            {
                throw new InvalidOperationException("A success result cannot have an error.");
            }

            if (!isSuccess && error == null)
            {
                throw new InvalidOperationException("A failure result must have an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(Error error) => new(false, error);
        public static Result<T> Success<T>(T value) => new(value, true, null);
        public static Result<T> Failure<T>(Error error) => new(default, false, error);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
            => _value = value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed.");
    }
}
