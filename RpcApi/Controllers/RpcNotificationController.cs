using Application.Services.Interfaces;
using PRC.Models;
using PRC.Models.Enums;
using PRC.Models.Packets;
using RPC.Contracts.Attributes;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;
using RpcApi.Controllers.Base;

namespace RpcApi.Controllers;

public class RpcNotificationController : BaseApiController<RpcNotificationController>
{
    private readonly IServerNetworkComponent _serverNetworkComponent;
    private readonly ILogger<RpcNotificationController> _logger;

    public RpcNotificationController(
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

    [Rpc((int)NotificationServerEnum.NotifySingle)]
    public async Task NotifySingle(UserClient client, NotifySinglePacket packet)
    {
        try
        {
            var notificationAggregator = GetService<INotificationAggregator>();
            var result = await notificationAggregator.NotifySingle(packet.Dto!);

            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.IsSuccess ? null : result.Error
            });

            _logger.LogInformation($"NotifySingle completed for client {client.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in NotifySingle for client {client.Id}");
            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    [Rpc((int)NotificationServerEnum.NotifyMultiple)]
    public async Task NotifyMultiple(UserClient client, NotifyMultiplePacket packet)
    {
        try
        {
            var notificationAggregator = GetService<INotificationAggregator>();
            var result = await notificationAggregator.NotifyMultiple(packet.UserListingPairs!);

            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.IsSuccess ? null : result.Error?.ToString()
            });

            _logger.LogInformation($"NotifyMultiple completed for client {client.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in NotifyMultiple for client {client.Id}");
            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = false,
                ErrorMessage = "Internal server error"
            });
        }
    }

    [Rpc((int)NotificationServerEnum.SendVerificationCode)]
    public async Task SendVerificationCode(UserClient client, SendVerificationCodePacket packet)
    {
        try
        {
            //debug
            Console.WriteLine(packet.EmailCode!.Token);

            var notificationAggregator = GetService<INotificationAggregator>();
            var result = await notificationAggregator.NotifySingle(packet.EmailCode);

            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.IsSuccess ? null : result.Error
            });

            _logger.LogInformation($"SendVerificationCode completed for client {client.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in SendVerificationCode for client {client.Id}");
            await _serverNetworkComponent.SendAsync(client, new ResultPacket
            {
                IsSuccess = false,
                ErrorMessage = "Internal server error"
            });
        }
    }
}