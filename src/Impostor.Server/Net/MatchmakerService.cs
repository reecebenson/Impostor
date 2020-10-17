using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Agones;
using Grpc.Core;
using Impostor.Server.Data;
using Impostor.Server.Net.Manager;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Impostor.Server.Net
{
    internal class MatchmakerService : IHostedService
    {
        private readonly ILogger<MatchmakerService> _logger;
        private readonly ServerConfig _serverConfig;
        private readonly ServerRedirectorConfig _redirectorConfig;
        private readonly IMatchmaker _matchmaker;
        private readonly AgonesSDK _agones;

        public MatchmakerService(
            ILogger<MatchmakerService> logger,
            IOptions<ServerConfig> serverConfig,
            IOptions<ServerRedirectorConfig> redirectorConfig,
            IMatchmaker matchmaker,
            AgonesSDK agones)
        {
            _logger = logger;
            _serverConfig = serverConfig.Value;
            _redirectorConfig = redirectorConfig.Value;
            _matchmaker = matchmaker;
            _agones = agones;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(_serverConfig.ListenIp), _serverConfig.ListenPort);

            await _matchmaker.StartAsync(endpoint);

            if (_redirectorConfig.Enabled && _redirectorConfig.Master == false)
            {
                var result = _agones.ReadyAsync().Result;
                if (result.StatusCode != StatusCode.OK)
                {
                    Log.Fatal("Failed to set agones ready state");
                }
            }
            
            _logger.LogInformation("Matchmaker is listening on {0}:{1}, the public server ip is {2}:{3}.", 
                endpoint.Address, 
                endpoint.Port,
                _serverConfig.PublicIp, 
                _serverConfig.PublicPort);

            if (_redirectorConfig.Enabled)
            {
                _logger.LogWarning(_redirectorConfig.Master
                    ? "Server redirection is enabled as master, this instance will redirect clients to other nodes."
                    : "Server redirection is enabled as node, this instance will accept clients.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Matchmaker is shutting down!");
            await _matchmaker.StopAsync();
        }
    }
}