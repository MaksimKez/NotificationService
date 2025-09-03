namespace RPC.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class RpcAttribute : Attribute
{
    public int OperationId { get; }

    public RpcAttribute(int operationId)
    {
        OperationId = operationId;
    }
}