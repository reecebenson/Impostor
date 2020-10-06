using System;

namespace Impostor.Hazel.Events
{
    public readonly struct DisconnectedEventArgs
    {
        public DisconnectedEventArgs(Exception exception)
        {
            Exception = exception;
        }
        
        public Exception Exception { get; }
    }
}