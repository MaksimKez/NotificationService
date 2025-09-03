using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace PRC.Models;

public class UserClient : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly Channel<byte[]> _sendChannel;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<UserClient> _logger;

    public string Id { get; }
    public string? UserId { get; set; }
    public bool IsConnected => _tcpClient.Connected && !_cts.IsCancellationRequested;

    public UserClient(TcpClient tcpClient, ILogger<UserClient> logger)
    {
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
        _stream = tcpClient.GetStream();
        _sendChannel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        _logger = logger;
        Id = Guid.NewGuid().ToString("N");
    }

    public async Task StartAsync(Func<UserClient, Task> receiveLoopStarter, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

        _ = Task.Run(() => SendLoopAsync(linked.Token), cancellationToken);

        await receiveLoopStarter(this).ConfigureAwait(false);
    }

    public ValueTask EnqueueOutgoingAsync(byte[] data)
    {
        return !_sendChannel.Writer.TryWrite(data)
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
                        _logger.LogWarning(ex, "Error writing to client {ClientId}", Id);
                        Cancel();
                        return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            Cancel();
            //just in case :)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled send loop error for client {ClientId}", Id);
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

    private void Cancel()
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
