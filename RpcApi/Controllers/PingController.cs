using PRC.Models;
using PRC.Models.Packets;
using RPC.Contracts.Attributes;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;
using RpcApi.Controllers.Base;

namespace RpcApi.Controllers;

public class PingController : BaseApiController<RpcNotificationController>
{
    private readonly IServerNetworkComponent _serverNetworkComponent;
    private readonly ILogger<RpcNotificationController> _logger;

    public PingController(
        IPacketProcessor packetProcessor,
        BaseServerNetwork baseServerNetwork,
        IServiceProvider serviceProvider,
        IServerNetworkComponent serverNetworkComponent,
        ILogger<RpcNotificationController> logger)
        : base(packetProcessor, baseServerNetwork, serviceProvider)
    {
        _serverNetworkComponent = serverNetworkComponent;
        _logger = logger;
    }


    [Rpc(1)]
    public async Task Ping(UserClient client, PingPacket packet)
    {
        await _serverNetworkComponent.SendAsync(client, new ResultPacket()
        {
            IsSuccess = true,
            ErrorMessage = null
        });
        await Task.CompletedTask;
        Console.WriteLine(packet.Message);

    }
}

public class PingPacket
{
    public string Message { get; set; }
}
