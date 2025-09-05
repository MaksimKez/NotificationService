using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PRC.Models;

namespace RPC.Contracts.Bases;

public class BaseServerNetwork(ILogger<BaseServerNetwork> logger)
{
    private readonly ConcurrentDictionary<string, UserClient> _connectedClients = new();
    
    public void AddClient(UserClient client)
    {
        _connectedClients[client.Id] = client;
    }

    public void RemoveClient(string clientId)
    {
        if (!_connectedClients.TryRemove(clientId, out _))
            throw new KeyNotFoundException($"Client with id {clientId} not found");
        
        logger.LogInformation("Client disconnected {ClientId}", clientId);
    }

    public UserClient? GetClient(string clientId)
    {
        return _connectedClients.GetValueOrDefault(clientId);
    }

    public IEnumerable<UserClient> GetAllClients()
    {
        return _connectedClients.Values;
    }

    public IEnumerable<UserClient> GetClientsByUserId(string userId)
    {
        return _connectedClients.Values.Where(c => c.UserId == userId);
    }
}
