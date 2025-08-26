namespace Application.Results;

public class ResultWithClass<TClass> : Result
    where TClass : class
{
    public TClass? Value { get; set; }
    
    public static ResultWithClass<TClass> Success(TClass value) 
            => new ResultWithClass<TClass> { Value = value, IsSuccess = true };
    
}