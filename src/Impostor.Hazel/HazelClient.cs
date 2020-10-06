using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
using Impostor.Hazel.Data;
using Impostor.Hazel.Events;

namespace Impostor.Hazel
{
    public partial class HazelClient : IDisposable
    {
        private readonly HazelServer _server;
        private bool _isFirst = true;
        private Task _executingTask;

        public HazelClient(HazelServer server, IPEndPoint remoteEndPoint)
        {
            _server = server;
            
            RemoteEndPoint = remoteEndPoint;
            Pipeline = new Pipe();
        }
        
        public IPEndPoint RemoteEndPoint { get; }
        internal Pipe Pipeline { get; }

        public Task StartAsync()
        {
            // Store the task we're executing
            _executingTask = ReadAsync();

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public void Stop()
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }
            
            // Cancel reader.
            Pipeline.Reader.CancelPendingRead();
            
            // Remove references.
            Dispose();
        }

        private async Task ReadAsync()
        {
            while (true)
            {
                var result = await Pipeline.Reader.ReadAsync();
                if (result.IsCanceled)
                {
                    // The read was canceled.
                    break;
                }
                
                var buffer = result.Buffer;

                // In the event that no message is parsed successfully, mark consumed
                // as nothing and examined as the entire buffer.
                var consumed = buffer.Start;
                var examined = buffer.End;

                try
                {
                    while (MessageReaderWriter.TryParseMessage(buffer.Slice(consumed), out consumed, out examined, out var type, out var message))
                    {
                        await ProcessMessageAsync(type, message);
                    }
                }
                catch (Exception e)
                {
                    Stop();

                    await OnDisconnectedAsync(e);
                    break;
                }
                finally
                {
                    Pipeline.Reader.AdvanceTo(consumed, examined);
                }
            }
        }

        private async Task ProcessMessageAsync(MessageType type, MessageReader message)
        {
            // Check if the first message received is the hello packet.
            if (_isFirst)
            {
                _isFirst = false;
                
                if (type != MessageType.Hello)
                {
                    Stop();
                    return;
                }

                // Message type byte is already removed from the payload.
                // Also slice the next 3 bytes to get handshake data.
                await _server.OnConnectionAsync(this, message.Slice(3));
            }
            
            // Process message.
            switch (type)
            {
                case MessageType.Reliable:
                    throw new NotImplementedException();
                case MessageType.Acknowledgement:
                    throw new NotImplementedException();
                case MessageType.Ping:
                    throw new NotImplementedException();
                case MessageType.Hello:
                    await AcknowledgeReliableMessageAsync(message);
                    break;
                case MessageType.Disconnect:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task OnDisconnectedAsync(Exception exception = null)
        {
            if (Disconnected != null)
            {
                await Disconnected(new DisconnectedEventArgs(exception));
            }
        }

        public void Dispose()
        {
            _server.RemoveClient(RemoteEndPoint);
        }
        
        public event Func<DisconnectedEventArgs, Task> Disconnected;
    }
}