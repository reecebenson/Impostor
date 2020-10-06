using System;
using System.Buffers.Binary;

namespace Impostor.Hazel
{
    public class MessageReader
    {
        public MessageReader(ReadOnlyMemory<byte> payload)
        {
            Payload = payload;
            Position = 0;
        }
        
        public int Position { get; set; }
        public int Length => Payload.Length;
        public ReadOnlyMemory<byte> Payload { get; }

        public MessageReader Slice(int start)
        {
            return new MessageReader(Payload.Slice(start));
        }

        public ushort ReadUInt16()
        {
            var result = BinaryPrimitives.ReadUInt16BigEndian(Payload.Span.Slice(Position));
            Position += 2;
            return result;
        }
    }
}