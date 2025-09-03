using System.Reflection;
using Application.Services;
using Application.Services.Interfaces;
using PRC.Models.Enums;
using PRC.Models.Packets;
using PRC.Models.Settings;
using RPC.Contracts.Attributes;
using RPC.Contracts.Bases;
using RPC.Contracts.Interfaces;
using RPC.Network;
using RPC.Network.DiRelated;
using RpcApi.Controllers;
using RpcApi.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RpcSettings>(builder.Configuration.GetSection("RpcSettings"));

builder.Services.AddSingleton<IPacketProcessor, PacketProcessor>();
builder.Services.AddSingleton<IOperationRegistry, OperationRegistry>();
builder.Services.AddSingleton<IServerNetworkComponent, ServerNetworkComponent>();
builder.Services.AddSingleton<BaseServerNetwork>();

builder.Services.AddHostedService<TcpHostedService>();

builder.Services.AddRpcControllersFromAssemblies(Assembly.GetExecutingAssembly(), typeof(RpcNotificationController).Assembly);

builder.Services.AddScoped<INotificationAggregator, NotificationAggregator>();

builder.Services.AddSingleton(builder.Services);

//for health checks in future
builder.Services.AddControllers();

var app = builder.Build();

RpcAutoRegistrar.RegisterRpcHandlers(app.Services);

var opReg = app.Services.GetRequiredService<IOperationRegistry>();
opReg.Register((int)NotificationServerEnum.Result, typeof(ResultPacket));

opReg.Register((int)NotificationServerEnum.NotifyMultiple, typeof(NotifyMultiplePacket));
opReg.Register((int)NotificationServerEnum.NotifySingle, typeof(NotifySinglePacket));
opReg.Register((int)NotificationServerEnum.SendVerificationCode, typeof(SendVerificationCodePacket));

app.MapControllers();

app.Run();
