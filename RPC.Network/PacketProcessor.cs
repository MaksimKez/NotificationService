using System.Collections.Concurrent;
using System.Text.Json;
using PRC.Models;
using RPC.Contracts.Interfaces;

namespace RPC.Network;

public class PacketProcessor : IPacketProcessor
{
    private readonly ConcurrentDictionary<Type, Func<UserClient, object, Task>> _handlers = new();
    private readonly ConcurrentDictionary<int, Type> _opIdToPacketType = new();


    public void RegisterHandler<TPacket>(Func<UserClient, TPacket, Task> handler) where TPacket : class
    {
        _handlers[typeof(TPacket)] = async (client, packet) =>
        {
            if (packet is TPacket typedPacket)
            {
                await handler(client, typedPacket).ConfigureAwait(false);
            }
        };
    }


    public void RegisterHandler(Type packetType, int operationId, Func<UserClient, object, Task> handler)
    {
        _opIdToPacketType[operationId] = packetType;
        _handlers[packetType] = handler;
    }


    public async Task ProcessPacketAsync<TPacket>(UserClient client, TPacket packet) where TPacket : class
    {
        try
        {
            if (_handlers.TryGetValue(typeof(TPacket), out var handler))
            {
                await handler(client, packet).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"No handler registered for {typeof(TPacket)}");
            }
        }
        catch
        {
            //debug
            throw;
        }
    }

    public async Task ProcessRawPacketAsync(UserClient client, int operationId, Guid requestId, string packetData)
    {
        try
        {
            if (!_opIdToPacketType.TryGetValue(operationId, out var packetType) || packetType == null)
            {
                throw new Exception($"No handler registered for operation id {operationId}");
            }


            var packet = JsonSerializer.Deserialize(packetData, packetType);
            if (packet == null)
            {
                throw new Exception($"Failed to deserialize packet for operation id {operationId}");
            }


            if (_handlers.TryGetValue(packetType, out var handler))
            {
                await handler(client, packet).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"No handler registered for packet type {packetType}");
            }
        }
        catch
        {
            //debug
            throw;
        }
    }
}