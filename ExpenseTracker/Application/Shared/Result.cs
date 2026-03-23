namespace ExpenseTracker.Application.Shared;

public struct Result<T>
{
    public Result(T value, bool ısSuccess, IDictionary<string, string[]> errors)
    {
        Value = value;
        IsSuccess = ısSuccess;
        Errors = errors;
    }

    public T Value { get; }
    public bool IsSuccess { get; }
    public IDictionary<string, string[]> Errors { get; }

    public static Result<T> Succeed(T value)
    {
        return new(value, false, null);
    }
    public static Result<T> Failed(T value, IDictionary<string, string[]> errors)
    {
        return new(value, false, errors);
    }

}
