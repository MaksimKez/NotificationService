using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace PRC.Models;

public class UserClient(TcpClient tcpClient) : IDisposable
{
    private readonly TcpClient _tcpClient = 
        tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        
    private readonly NetworkStream _stream = tcpClient.GetStream();
    private readonly Channel<byte[]> _sendChannel 
        = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        
    private readonly CancellationTokenSource _cts = new();
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public bool IsConnected => _tcpClient.Connected && !_cts.IsCancellationRequested;

    public async Task StartAsync(Func<UserClient, Task> receiveLoopStarter, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

        _ = Task.Run(() => SendLoopAsync(linked.Token), cancellationToken);

        await receiveLoopStarter(this).ConfigureAwait(false);
    }

    public ValueTask EnqueueOutgoingAsync(byte[] data)
    {
        return !_sendChannel.Writer.TryWrite(data)
            // fallback to async write
            ? _sendChannel.Writer.WriteAsync(data)
            : ValueTask.CompletedTask;
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            var reader = _sendChannel.Reader;
            while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (reader.TryRead(out var item))
                {
                    try
                    {
                        await _stream.WriteAsync(item, 0, item.Length, cancellationToken).ConfigureAwait(false);
                        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Cancel();
                        return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Cancel();
        }
        catch (Exception ex)
        {
            Cancel();
        }
    }

    public async Task<(int OperationId, Guid RequestId, string Payload)> ReadEnvelopeAsync(CancellationToken cancellationToken)
    {
        var lenBuf = new byte[4];
        await ReadExactAsync(_stream, lenBuf, cancellationToken).ConfigureAwait(false);
        var length = BitConverter.ToInt32(lenBuf, 0);
        if (length <= 0)
            throw new InvalidDataException("Invalid envelope length");

        var buffer = new byte[length];
        await ReadExactAsync(_stream, buffer, cancellationToken).ConfigureAwait(false);

        if (length < 4 + 16)
            throw new InvalidDataException("Envelope too small");

        var opId = BitConverter.ToInt32(buffer, 0);
        var guidBytes = new byte[16];
        Array.Copy(buffer, 4, guidBytes, 0, 16);
        var requestId = new Guid(guidBytes);

        var payloadLen = length - 4 - 16;
        var payload = Encoding.UTF8.GetString(buffer, 4 + 16, payloadLen);

        return (opId, requestId, payload);
    }

    private static async Task ReadExactAsync(Stream s, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await s.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken).ConfigureAwait(false);
            if (read == 0)
                throw new EndOfStreamException("Stream closed while reading");
            offset += read;
        }
    }

    public void Cancel()
    {
        try { _cts.Cancel(false); } catch { }
        try { _tcpClient.Close(); } catch { }
    }

    public void Dispose()
    {
        try { _cts.Cancel(); } catch { }
        try { _stream.Dispose(); } catch { }
        try { _tcpClient.Dispose(); } catch { }
    }
}