namespace Application.Results;

public class ResultWithValue<TValue>
    : Result
where TValue : struct
{
    public TValue Value { get; init; }
    
    public static ResultWithValue<TValue> Success(TValue value) 
        => new ResultWithValue<TValue> { Value = value, IsSuccess = true };
}