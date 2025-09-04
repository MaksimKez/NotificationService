using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PRC.Models;
using RPC.Contracts.Attributes;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;

namespace RPC.Network;

public class ServerNetworkComponent(
    BaseServerNetwork baseServerNetwork,
    IOperationRegistry operationRegistry,
    ILogger<ServerNetworkComponent> logger)
    : IServerNetworkComponent
{
    public async Task SendAsync<TPacket>(UserClient client, TPacket packet) where TPacket : class
    {
        try
        {
            logger.LogInformation("Sending RPC packet");
            if (!client.IsConnected)
            {
                return;
            }

            var packetType = packet.GetType();
            int opId;
            if (!operationRegistry.TryGetOperationId(packetType, out opId))
            {
                logger.LogWarning("No operation id mapped for response packet type {PacketType}. Using opId=0.", packetType.FullName);
                opId = 0;
            }

                                    //i wanted to see smt new to make sure that
                                    //the result packet is real, ill keep it as it is
                                    //because it just "fun" part of the project
            var json = JsonSerializer.Serialize(packet) + "\n some changes";
            var requestId = Guid.NewGuid();
            var data = TcpHostedService.BuildEnvelopeBytes(opId, requestId, json);
            logger.LogInformation("data is formed");
            await client.EnqueueOutgoingAsync(data).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send packet to client {ClientId}", client.Id);
            throw;
        }
    }

    public async Task BroadcastAsync<TPacket>(TPacket packet) where TPacket : class
    {
        var clients = baseServerNetwork.GetAllClients().Where(c => c.IsConnected);
        var tasks = clients.Select(client => SendAsync(client, packet));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task SendToUserAsync<TPacket>(string userId, TPacket packet) where TPacket : class
    {
        var userClients = baseServerNetwork.GetClientsByUserId(userId).Where(c => c.IsConnected);
        var tasks = userClients.Select(client => SendAsync(client, packet));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}