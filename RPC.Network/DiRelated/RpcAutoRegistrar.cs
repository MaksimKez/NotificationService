using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PRC.Models;
using RPC.Contracts.Attributes;
using RPC.Contracts.Interfaces;

namespace RPC.Network.DiRelated;

public static class RpcAutoRegistrar
{
    public static void RegisterRpcHandlers(IServiceProvider rootProvider)
    {
        var packetProcessor = rootProvider.GetRequiredService<PacketProcessor>();
        var operationRegistry = rootProvider.GetRequiredService<IOperationRegistry>();
        var services = rootProvider.GetRequiredService<IServiceCollection>() ?? null;

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in assemblies)
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

            foreach (var type in types)
            {
                if (type == null) continue;

                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                foreach (var mi in methods)
                {
                    var attr = mi.GetCustomAttribute<RpcAttribute>();
                    if (attr == null) continue;

                    var parameters = mi.GetParameters();
                    if (parameters.Length != 2)
                    {
                        throw new InvalidOperationException($"Method {mi.DeclaringType!.FullName}.{mi.Name} must have exactly 2 parameters: (UserClient, TPacket)");
                    }

                    if (parameters[0].ParameterType != typeof(UserClient))
                    {
                        throw new InvalidOperationException($"First parameter of {mi.DeclaringType!.FullName}.{mi.Name} must be UserClient");
                    }

                    var packetType = parameters[1].ParameterType;

                    Func<UserClient, object, Task> handler = async (client, packetObj) =>
                    {
                        using var scope = rootProvider.CreateScope();

                        var controller = scope.ServiceProvider.GetService(type) 
                                         ?? ActivatorUtilities.CreateInstance(scope.ServiceProvider, type);

                        var resultTask = (Task)mi.Invoke(controller, new[] { client, packetObj })!;
                        await resultTask.ConfigureAwait(false);
                    };

                    packetProcessor.RegisterHandler(packetType, attr.OperationId, handler);
                    operationRegistry.Register(attr.OperationId, packetType);
                }
            }
        }
    }
}