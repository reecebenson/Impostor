using System.Net;
using System.Threading.Tasks;
using Impostor.Hazel.Events;
using Serilog;

namespace Impostor.Hazel.App
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            // Create logger.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
            
            // Create server.
            var serverEndpoint = new IPEndPoint(IPAddress.Any, 22023);
            var server = new HazelServer(serverEndpoint);

            // Listen for events.
            server.Connection += ServerOnConnection;
            
            // Start server.
            await server.StartAndWaitAsync();
        }

        private static Task ServerOnConnection(ConnectionEventArgs e)
        {
            Log.Information("Server -> New connection {0}.", e.Client.RemoteEndPoint);
            
            e.Client.Disconnected += ClientOnDisconnected;
            
            return Task.CompletedTask;
        }

        private static Task ClientOnDisconnected(DisconnectedEventArgs e)
        {
            Log.Information(e.Exception, "Client -> Disconnected");
            
            return Task.CompletedTask;
        }
    }
}