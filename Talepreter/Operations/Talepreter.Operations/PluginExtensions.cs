using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Talepreter.Operations.Plugin;

namespace Talepreter.Operations
{
    public static class PluginExtensions
    {
        /// <summary>
        /// Call this method to call plugin registrations to DI
        /// operation is silent, just writes to console
        /// call this last
        /// </summary>
        public static IServiceCollection RegisterPlugins(this IServiceCollection services)
        {
            var pluginType = typeof(IPluginRegistration);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var implementerTypes = asm.GetTypes().Where(x => pluginType.IsAssignableFrom(x) && x.IsClass);
                foreach (var type in implementerTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(type) is IPluginRegistration instance)
                        {
                            instance.RegisterPlugin(services);
                            Log.Information($"PLUGIN: Plugin registration from type {type.FullName} was successful");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"PLUGIN: Plugin registration from type {type.FullName} failed: {ex.Message}");
                        Log.Error($"{ex.StackTrace}");
                    }
                }
            }
            return services;
        }
    }
}
