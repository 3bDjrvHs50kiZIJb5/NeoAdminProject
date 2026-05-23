namespace NeoAdmin.Blazor.Auth;

public sealed record ApiResult
{
    public int Code { get; init; }

    public string Message { get; init; } = string.Empty;

    public bool Succeeded => Code == 0;

    public static ApiResult Success(string message = "成功") => new()
    {
        Code = 0,
        Message = message
    };

    public static ApiResult Error(string message, int code = 5001) => new()
    {
        Code = code,
        Message = message
    };
}

public sealed record ApiResult<T>
{
    public int Code { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public bool Succeeded => Code == 0;

    public static ApiResult<T> Success(T data, string message = "成功") => new()
    {
        Code = 0,
        Message = message,
        Data = data
    };

    public static ApiResult<T> Error(string message, int code = 5001) => new()
    {
        Code = code,
        Message = message
    };
}
