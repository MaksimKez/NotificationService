using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RpcClient;

class Program
{
    static async Task Main(string[] args)
    {
        var host = "127.0.0.1";
        var port = 5005;

        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"Connected to {host}:{port}");

        using var stream = client.GetStream();

        int opId = 1;
        var requestId = Guid.NewGuid();
        Console.WriteLine("Sending request to RPC server...");

        var pingPacket = new
        {
            Message = "Hello from client",
            UserId = "887B8E05-67F1-456F-8103-F2DE7D8430E6"
        };

        string jsonPayload = JsonSerializer.Serialize(pingPacket);

        // Envelope: [length:int32][opId:int32][requestId:guid][payload:utf8]
        var payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var length = 4 + 16 + payloadBytes.Length;
        var buffer = new byte[4 + length];

        BitConverter.GetBytes(length).CopyTo(buffer, 0);
        BitConverter.GetBytes(opId).CopyTo(buffer, 4);
        requestId.ToByteArray().CopyTo(buffer, 8);
        Array.Copy(payloadBytes, 0, buffer, 24, payloadBytes.Length);

        await stream.WriteAsync(buffer);
        Console.WriteLine($"Sent PingPacket with requestId={requestId}");

        var lenBuf = new byte[4];
        await ReadExactAsync(stream, lenBuf);
        var respLength = BitConverter.ToInt32(lenBuf, 0);

        var respBuf = new byte[respLength];
        await ReadExactAsync(stream, respBuf);
        
        var respJson = Encoding.UTF8.GetString(respBuf, 20, respLength - 20);
        Console.WriteLine("Response: " + respJson);
    }

    static async Task ReadExactAsync(NetworkStream stream, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset);
            if (read == 0) throw new Exception("Connection closed");
            offset += read;
        }
    }
}

public class NotifySinglePacket
{
    public UserListingPairDto? Dto { get; set; } = null;
}

public class UserListingPairDto
{
    public UserDto User { get; set; }
    public ListingDto Listing { get; set; }
}

public class ListingDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public decimal AreaMeterSqr { get; set; }
    public int Rooms { get; set; }
    public int Floor { get; set; }
    public bool IsFurnished { get; set; }
    public bool PetsAllowed { get; set; }
    public bool HasBalcony { get; set; }
    public bool HasAppliances { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string? Email { get; set; }
    public string? TelegramId { get; set; }
}