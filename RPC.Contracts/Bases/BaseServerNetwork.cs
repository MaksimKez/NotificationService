using PRC.Models;

namespace RPC.Contracts.Bases;

public class BaseServerNetwork
{
    private readonly Dictionary<string, UserClient> _connectedClients = new();
    
    public void AddClient(UserClient client)
    {
        _connectedClients[client.Id] = client;
    }

    public void RemoveClient(string clientId)
    {
        if (_connectedClients.Remove(clientId))
        {
            return;
            //log
        }

        //for debug only
        throw new Exception();
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
        return _connectedClients.Values.Where(c => c.Id == userId);
    }
}
