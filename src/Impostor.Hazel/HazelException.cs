using System;
using System.Runtime.Serialization;

namespace Impostor.Hazel
{
    public class HazelException : Exception
    {
        public HazelException()
        {
        }

        protected HazelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HazelException(string message) : base(message)
        {
        }

        public HazelException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}