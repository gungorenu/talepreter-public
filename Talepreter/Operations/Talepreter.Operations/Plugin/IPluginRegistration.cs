using Microsoft.Extensions.DependencyInjection;

namespace Talepreter.Operations.Plugin
{
    /// <summary>
    /// to register a plugin, this type must be implemented by plugin
    /// must be a simple class with constructor and not abstract
    /// </summary>
    public interface IPluginRegistration
    {
        IServiceCollection RegisterPlugin(IServiceCollection services);
    }
}
