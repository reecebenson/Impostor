using System;
using System.Buffers;
using Impostor.Hazel.Data;

namespace Impostor.Hazel
{
    internal static class MessageReaderWriter
    {
        public static bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            out SequencePosition consumed, 
            out SequencePosition examined,
            out MessageType type,
            out MessageReader message)
        {
            var reader = new SequenceReader<byte>(input);
            
            // Read message type.
            if (!reader.TryRead(out var option))
            {
                consumed = input.Start;
                examined = input.End;
                type = MessageType.None;
                message = default;
                return false;
            }

            // Create MessageReader.
            var payload = input.Slice(reader.Position);
            reader.AdvanceToEnd();
            
            type = (MessageType) option;
            message = new MessageReader(payload.IsSingleSegment ? payload.First : payload.ToArray());

            // Mark as read.
            consumed = reader.Position;
            examined = consumed;

            return true;
        }
    }
}