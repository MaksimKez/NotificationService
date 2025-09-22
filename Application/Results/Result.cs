namespace Application.Results;

public class Result
{
    public string Error { get; init; }
    public bool IsSuccess { get; set; }

    public static Result Success()
            => new Result { IsSuccess = true };
    
    public static Result Failure(string error)
            => new Result { IsSuccess = false, Error = error };
}