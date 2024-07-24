using Orleans.Configuration;
using System.Net;
using System.Net.Sockets;

namespace Talepreter.Common.Orleans
{
    public static class OrleansSiloExtensions
    {
        private const string InvariantName = "Microsoft.Data.SqlClient";

        /// <summary>
        /// Sets up Orleans configuration, all silos are co-hosted
        /// </summary>
        public static void ConfigureTalepreterOrleans(this ISiloBuilder silo, string storageName, string siloName, int portBase = 1)
        {
            var clusteringConnString = EnvironmentVariableHandler.ReadEnvVar("OrleansClusterDBConnection");
            var storageConnString = EnvironmentVariableHandler.ReadEnvVar("DBConnection");
            var ipAddress = EnvironmentVariableHandler.ReadEnvVar("OrleansIPAddress");
            var clusterId = EnvironmentVariableHandler.ReadEnvVar("OrleansClusterId");
            var serviceId = EnvironmentVariableHandler.ReadEnvVar("OrleansServiceId");

            silo.Configure<SiloOptions>(options => options.SiloName = siloName );

            silo.UseAdoNetClustering(options => { options.Invariant = InvariantName; options.ConnectionString = clusteringConnString; })
                .Configure<ClusterOptions>(options => { options.ClusterId = clusterId; options.ServiceId = serviceId; });

            silo.Configure<EndpointOptions>(options =>
            {
                options.AdvertisedIPAddress = GetAdvertisedIpAddress(ipAddress);
                options.GatewayPort = 30000 + portBase;
                options.SiloPort = 11110 + portBase; 
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30000 + portBase);
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 11110 + portBase);
            });

            silo.AddAdoNetGrainStorage(storageName, options => { options.Invariant = InvariantName; options.ConnectionString = storageConnString; });
            silo.AddAdoNetGrainStorage("PluginStorage", options => { options.Invariant = InvariantName; options.ConnectionString = storageConnString; });
            silo.UseAdoNetReminderService(options => { options.Invariant = InvariantName; options.ConnectionString = storageConnString; });
        }

        public static void ConfigureTalepreterOrleansClient(this IClientBuilder builder, int portBase = 10)
        {
            var clusteringConnString = EnvironmentVariableHandler.ReadEnvVar("OrleansClusterDBConnection");
            var ipAddress = EnvironmentVariableHandler.ReadEnvVar("OrleansIPAddress");
            var clusterId = EnvironmentVariableHandler.ReadEnvVar("OrleansClusterId");
            var serviceId = EnvironmentVariableHandler.ReadEnvVar("OrleansServiceId");

            builder.UseAdoNetClustering(options => { options.Invariant = InvariantName; options.ConnectionString = clusteringConnString; })
                .Configure<ClusterOptions>(options => { options.ClusterId = clusterId; options.ServiceId = serviceId; });

            builder.Configure<EndpointOptions>(options =>
            {
                options.AdvertisedIPAddress = GetAdvertisedIpAddress(ipAddress);
                options.GatewayPort = 30000 + portBase;
                options.SiloPort = 11110 + portBase;
                options.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 30000 + portBase);
                options.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 11110 + portBase);
            });

        }

        private static IPAddress GetAdvertisedIpAddress(string ipAddress)
        {
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return IPAddress.Parse(ipAddress);
            }

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
