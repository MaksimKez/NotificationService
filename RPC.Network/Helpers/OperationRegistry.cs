using System.Collections.Concurrent;
using RPC.Contracts.Interfaces;

namespace RPC.Network.Helpers;

public class OperationRegistry : IOperationRegistry
{
    private readonly ConcurrentDictionary<int, Type> _opToType = new();
    private readonly ConcurrentDictionary<Type, int> _typeToOp = new();
    
    public void Register(int operationId, Type packetType)
    {
        _opToType[operationId] = packetType;
        _typeToOp[packetType] = operationId;
    }
    
    public bool TryGetPacketType(int operationId, out Type? packetType)
        => _opToType.TryGetValue(operationId, out packetType);
    public bool TryGetOperationId(Type packetType, out int operationId)
        => _typeToOp.TryGetValue(packetType, out operationId);
}