namespace RPC.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RpcAttribute(int operationId) : Attribute
{
    public int OperationId { get; } = operationId;
}