namespace AdessoLeague.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        // Fails fast on purpose: a broken success/error pairing is a programming mistake, not a
        // domain outcome, and returning it would defeat the point of the type.
        if (isSuccess == (error != Error.None))
        {
            throw new ArgumentException("A success cannot carry an error and a failure must carry one.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}
