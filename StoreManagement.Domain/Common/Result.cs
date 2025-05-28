namespace StoreManagement.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    
    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException(StrFaSD.Success_Result_Cannot_Have_Error_Msg);

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException(StrFaSD.Result_Failure_Must_Have_Error_Msg);

        IsSuccess = isSuccess;
        Error = error;
    }
    
    public bool IsFailure => !IsSuccess;
    public static Result Success() => new Result(true, string.Empty);
    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
    public static Result<T> Failure<T>(string error) => new Result<T>(default, false, error);
}

public class Result<T> : Result
{
    public readonly T _value;

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        _value = value;
    }
    
    public T Value
    {
        get
        {
            if (!IsSuccess)
                throw new InvalidOperationException(StrFaSD.Cannot_Retrieve_Value_From_Failed_Result);

            return _value;
        }
    }
    
    public static implicit operator Result<T>(T value) => Success(value);
}


