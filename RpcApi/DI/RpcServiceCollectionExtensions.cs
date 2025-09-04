using System.Reflection;
using RPC.Contracts.Attributes;

namespace RpcApi.DI;

public static class RpcServiceCollectionExtensions
{
    public static IServiceCollection AddRpcControllersFromAssemblies(
        this IServiceCollection services, params Assembly[]? assemblies)
        => services.AddRpcControllersFromAssemblies(_ => true, assemblies);

    private static IServiceCollection AddRpcControllersFromAssemblies(this IServiceCollection services, Func<Type, bool> typeFilter, params Assembly[]? assemblies)
    {
        ArgumentNullException.ThrowIfNull(typeFilter);

        var toScan = assemblies is { Length: > 0 }
            ? assemblies
            : AppDomain.CurrentDomain.GetAssemblies();

        foreach (var asm in toScan)
        {
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }
            catch
            { 
                continue;
            }

            foreach (var t in types)
            {
                if (!t.IsClass || t.IsAbstract) continue;
                if (t.IsGenericTypeDefinition) continue;

                if (t.Namespace != null &&
                    (t.Namespace.StartsWith("System", StringComparison.Ordinal)
                     || t.Namespace.StartsWith("Microsoft", StringComparison.Ordinal)
                     || t.Namespace.StartsWith("netstandard", StringComparison.Ordinal)))
                {
                    continue;
                }

                if (!typeFilter(t)) continue;

                var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var hasRpc = methods.Any(m => m.GetCustomAttribute<RpcAttribute>(inherit: false) != null);

                if (!hasRpc) continue;

                var alreadyRegistered = services.Any(sd => sd.ServiceType == t);
                if (!alreadyRegistered)
                {
                    services.AddTransient(t);
                }
            }
        }
        return services;
    }
}