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

        BitConverter.GetBytes(length).CopyTo(buffer, 0);
        BitConverter.GetBytes(opId).CopyTo(buffer, 4);
        requestId.ToByteArray().CopyTo(buffer, 8);
        Array.Copy(payloadBytes, 0, buffer, 8 + 16, payloadBytes.Length);

        return buffer;
    }
}