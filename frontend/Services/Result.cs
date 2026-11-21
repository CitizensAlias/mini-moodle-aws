namespace minimoodle.Services;

public sealed class Result
{
    public bool IsSuccess { get; }
    public ResultError? ResultError { get; }

    private Result(bool isSuccess, ResultError? resultError)
    {
        IsSuccess = isSuccess;
        ResultError = resultError;
    }

    public static Result Success()
    {
        return new Result(true, null);
    }

    public static Result Failure(ResultError errorMessage)
    {
        return new Result(false, errorMessage);
    }
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Content { get; }
    public ResultError? ResultError { get; }
    
    private Result(bool isSuccess, T? content, ResultError? resultError)
    {
        IsSuccess = isSuccess;
        Content = content;
        ResultError = resultError;
    }

    public static Result<T> Success(T content)
    {
        return new Result<T>(true, content, null);
    }

    public static Result<T> Failure(ResultError resultError)
    {
        return new Result<T>(false, default, resultError);
    }
}
