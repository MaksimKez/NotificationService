namespace Application.Results;

public class ResultWithClass<TClass> : Result
    where TClass : class
{
    public bool IsPartialFailure { get;  set; }
    public TClass? Value { get; set; }
    
    public static ResultWithClass<TClass> Success(TClass value) 
        => new ResultWithClass<TClass> { Value = value, IsSuccess = true };
    
    public static ResultWithClass<TClass> PartialFailure(TClass value)
        => new ResultWithClass<TClass> { Value = value, IsSuccess = false, IsPartialFailure = true };
}
