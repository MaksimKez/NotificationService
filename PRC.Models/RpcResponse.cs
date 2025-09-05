namespace PRC.Models;

public class RpcResponse
{
    public bool IsSuccess { get; set; }
    public string? Data { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; }
}