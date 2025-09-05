using System.Buffers.Binary;
using System.Text;

namespace RPC.Network.Helpers;

public static class EnvelopeBuilder
{
    /// <summary>
    /// (4) +opId (4) + requestId (16) + payload (utf8)
    /// </summary>
    /// <param name="opId"> operation id </param>
    /// <param name="requestId"> request id </param>
    /// <param name="jsonPayload"> payload itself</param>
    /// <returns></returns>
    public static byte[] BuildEnvelopeBytes(int opId, Guid requestId, string jsonPayload)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(jsonPayload);
        var length = 4 + 16 + payloadBytes.Length;
        var buffer = new byte[4 + length];

        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(0, 4), length);
        BinaryPrimitives.WriteInt32BigEndian(buffer.AsSpan(4, 4), opId);

        requestId.TryWriteBytes(buffer.AsSpan(8, 16));
        
        payloadBytes.CopyTo(buffer.AsSpan(24));

        return buffer;
    }
}
