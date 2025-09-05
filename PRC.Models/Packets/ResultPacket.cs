namespace PRC.Models.Packets;

public class ResultPacket
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}