using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;

namespace RpcApi.Controllers.Base;

public abstract class BaseApiController<T>(
    IPacketProcessor packetProcessor,
    BaseServerNetwork baseServerNetwork,
    IServiceProvider serviceProvider)
    where T : class
{
    protected readonly IPacketProcessor PacketProcessor = packetProcessor;
    protected readonly BaseServerNetwork BaseServerNetwork = baseServerNetwork;
    protected readonly IServiceProvider ServiceProvider = serviceProvider;

    protected TService GetService<TService>() where TService : notnull
    {
        return ServiceProvider.GetRequiredService<TService>();
    }
}
