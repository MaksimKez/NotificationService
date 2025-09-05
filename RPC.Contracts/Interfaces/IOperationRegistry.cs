namespace RPC.Contracts.Interfaces;

public interface IOperationRegistry
{
    void Register(int operationId, Type packetType);
    bool TryGetPacketType(int operationId, out Type? packetType);
    bool TryGetOperationId(Type packetType, out int operationId);
}