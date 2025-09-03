using PRC.Models;

namespace RPC.Contracts.Interfaces;

public interface IServerNetworkComponent
{
    Task SendAsync<TPacket>(UserClient client, TPacket packet) where TPacket : class;
    Task BroadcastAsync<TPacket>(TPacket packet) where TPacket : class;
    Task SendToUserAsync<TPacket>(string userId, TPacket packet) where TPacket : class;
}