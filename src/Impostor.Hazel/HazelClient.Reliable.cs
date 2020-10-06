using System;
using System.Threading.Tasks;

namespace Impostor.Hazel
{
    public partial class HazelClient
    {
        private async Task ProcessReliableMessageAsync()
        {
            
        }

        private async Task AcknowledgeReliableMessageAsync(MessageReader message)
        {
            var id = message.ReadUInt16();

            await SendAcknowledgement(id);
            Console.WriteLine("asd");
        }

        private async Task SendAcknowledgement(ushort id)
        {
            // TODO: Do.
        }
    }
}