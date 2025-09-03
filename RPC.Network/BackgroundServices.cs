using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRC.Models;
using PRC.Models.Settings;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;

namespace RPC.Network;

    public class TcpHostedService : BackgroundService
    {
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PacketProcessor _packetProcessor;
        private readonly BaseServerNetwork _baseServerNetwork;
        private readonly IOperationRegistry _operationRegistry;
        private readonly IPAddress _listenAddress;
        private readonly int _port;

        private TcpListener? _listener;

        public TcpHostedService(
            IServiceProvider serviceProvider,
            PacketProcessor packetProcessor,
            BaseServerNetwork baseServerNetwork,
            IOperationRegistry operationRegistry,
            ILogger<TcpHostedService> logger,
            IOptions<RpcSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _packetProcessor = packetProcessor;
            _baseServerNetwork = baseServerNetwork;
            _operationRegistry = operationRegistry;
            _logger = logger;

            var host = settings.Value.Host ?? "0.0.0.0";
            _listenAddress = IPAddress.Parse(host);
            _port = settings.Value.Port ?? 5000;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _listener = new TcpListener(_listenAddress, _port);
            _listener.Start();
            _logger.LogInformation("TCP RPC server listening on {Address}:{Port}", _listenAddress, _port);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                    _ = Task.Run(() => HandleClientAsync(tcpClient, stoppingToken), stoppingToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Listener failed");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken serverCancellation)
        {
            var userClient = new UserClient(tcpClient);

            try
            {
                _baseServerNetwork.AddClient(userClient);
                _logger.LogInformation("New client connected {ClientId}", userClient.Id);

                // receive loop
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serverCancellation);
                var receiveLoopTask = Task.Run(() => ReceiveLoopAsync(userClient, linkedCts.Token), linkedCts.Token);

                // start send loop is triggered by userClient.StartAsync
                await Task.CompletedTask;

                // await receive completion
                await receiveLoopTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client {ClientId}", userClient.Id);
            }
            finally
            {
                try
                {    
                    _baseServerNetwork.RemoveClient(userClient.Id);
                }
                catch { }

                try { userClient.Dispose(); } catch { }
                _logger.LogInformation("Client disconnected {ClientId}", userClient.Id);
            }
        }

        private async Task ReceiveLoopAsync(UserClient userClient, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && userClient.IsConnected)
                {
                    (int opId, Guid requestId, string payload) envelope;
                    try
                    {
                        envelope = await userClient.ReadEnvelopeAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (EndOfStreamException)
                    {
                        //client closed
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to read envelope from {ClientId}", userClient.Id);
                        break;
                    }

                    // process in background so receiver continues reading
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _packetProcessor.ProcessRawPacketAsync(userClient, envelope.opId, envelope.requestId, envelope.payload).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing packet opId={OpId} from client {ClientId}", envelope.opId, userClient.Id);

                            var errorObj = new { IsSuccess = false, ErrorMessage = ex.Message, Correlation = envelope.requestId };
                            var json = JsonSerializer.Serialize(errorObj);
                            var data = BuildEnvelopeBytes(0, envelope.requestId, json);
                            try { await userClient.EnqueueOutgoingAsync(data); } catch { }
                        }
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Receive loop crashed for {ClientId}", userClient.Id);
            }
        }


        //length+opId+requestId+payload (utf8)
        // 4+4+16+payload
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

