namespace Impostor.Hazel.Data
{
    public enum MessageType
    {
        None = 0,
        Reliable = 1,
        
        /// <summary>
        ///     Hello message for initiating communication.
        /// </summary>
        Hello = 8,

        /// <summary>
        ///     Message for discontinuing communication.
        /// </summary>
        Disconnect = 9,

        /// <summary>
        ///     Message acknowledging the receipt of a message.
        /// </summary>
        Acknowledgement = 10,

        /// <summary>
        ///     Message that is part of a larger, fragmented message.
        /// </summary>
        Fragment = 11,

        /// <summary>
        /// A single byte of continued existence
        /// </summary>
        Ping = 12,
    }
}