using PRC.Models;
using PRC.Models.Packets;
using RPC.Contracts.Attributes;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;
using RpcApi.Controllers.Base;

namespace RpcApi.Controllers;

public class PingController(
    IPacketProcessor packetProcessor,
    BaseServerNetwork baseServerNetwork,
    IServiceProvider serviceProvider,
    IServerNetworkComponent serverNetworkComponent,
    ILogger<RpcNotificationController> logger)
    : BaseApiController<RpcNotificationController>(packetProcessor, baseServerNetwork, serviceProvider)
{

    [Rpc(1)]
    public async Task Ping(UserClient client, PingPacket packet)
    {
        logger.LogInformation($"Ping completed for client {client.Id}");
        await serverNetworkComponent.SendAsync(client, new ResultPacket()
        {
            IsSuccess = true,
            ErrorMessage = null
        });
        Console.WriteLine(packet.Message);
    }
}