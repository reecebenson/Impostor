using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Impostor.Hazel.Events;
using Serilog;

namespace Impostor.Hazel
{
    public class HazelServer
    {
        private static readonly ILogger Logger = Log.ForContext<HazelServer>();
        
        private readonly ConcurrentDictionary<IPEndPoint, HazelClient> _clients;
        private readonly UdpClient _socket;
        private readonly CancellationTokenSource _stoppingCts;
        private Task _executingTask;
        
        public HazelServer(IPEndPoint endPoint)
        {
            _clients = new ConcurrentDictionary<IPEndPoint, HazelClient>();
            
            _socket = new UdpClient(endPoint)
            {
                DontFragment = true
            };
            
            _stoppingCts = new CancellationTokenSource();
            _stoppingCts.Token.Register(() =>
            {
                _socket.Dispose();
            });
        }

        public Task StartAsync()
        {
            // Store the task we're executing
            _executingTask = ListenAsync();

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public async Task StartAndWaitAsync()
        {
            await StartAsync();
            await _executingTask;
        }

        public async Task StopAsync()
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }
            
            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the timeout triggers
                await Task.WhenAny(_executingTask, Task.Delay(TimeSpan.FromSeconds(5)));
            }
        }

        private async Task ListenAsync()
        {
            while (!_stoppingCts.IsCancellationRequested)
            {
                var data = await _socket.ReceiveAsync();
                if (data.Buffer.Length == 0)
                {
                    Logger.Fatal("Hazel read 0 bytes from UDP server socket.");
                    continue;    
                }
                
                // Get client from active clients
                if (!_clients.TryGetValue(data.RemoteEndPoint, out var client))
                {
                    // Create new client
                    client = new HazelClient(this, data.RemoteEndPoint);
                 
                    // Store the client
                    if (!_clients.TryAdd(data.RemoteEndPoint, client))
                    {
                        throw new HazelException("Failed to add a connection. This should never happen.");
                    }
                    
                    // Activate the reader loop of the client
                    await client.StartAsync();
                }
                
                // Write received bytes to the client
                await client.Pipeline.Writer.WriteAsync(data.Buffer);
            }
        }

        internal async Task SendAsync(byte[] data, int length, IPEndPoint endPoint)
        {
            // https://github.com/dotnet/runtime/issues/33417
            await _socket.SendAsync(data, data.Length, endPoint);
        }

        internal void RemoveClient(IPEndPoint endPoint)
        {
            _clients.TryRemove(endPoint, out _);
        }

        internal async Task OnConnectionAsync(HazelClient client, MessageReader handshakeData)
        {
            if (Connection != null)
            {
                await Connection(new ConnectionEventArgs(client, handshakeData));
            }
        }
        
        public event Func<ConnectionEventArgs, Task> Connection;
    }
}