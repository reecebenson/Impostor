namespace Impostor.Hazel.Events
{
    public readonly struct ConnectionEventArgs
    {
        public ConnectionEventArgs(HazelClient client, MessageReader handshakeData)
        {
            Client = client;
            HandshakeData = handshakeData;
        }
        
        public HazelClient Client { get; }
        public MessageReader HandshakeData { get; }
    }
}