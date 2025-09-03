using PRC.Models;

namespace RPC.Contracts.Interfaces;

public interface IPacketProcessor
{
    void RegisterHandler<TPacket>(Func<UserClient, TPacket, Task> handler) where TPacket : class;
    void RegisterHandler(Type packetType, int operationId, Func<UserClient, object, Task> handler);
    Task ProcessPacketAsync<TPacket>(UserClient client, TPacket packet) where TPacket : class;
    Task ProcessRawPacketAsync(UserClient client, int operationId, Guid requestId, string packetData);
}