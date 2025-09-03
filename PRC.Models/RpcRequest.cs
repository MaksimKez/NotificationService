namespace PRC.Models;

public class RpcRequest
{
    public int OperationId { get; set; }
    public string PacketData { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string ClientId { get; set; } = null!;
}