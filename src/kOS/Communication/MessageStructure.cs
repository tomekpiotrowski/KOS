using System;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;
using kOS.Safe;
using kOS.Serialization;

namespace kOS.Communication
{
    public class MessageStructure : Structure
    {
        public Message Message { get; private set; }
        private SharedObjects shared;

        public MessageStructure(Message message, SharedObjects shared)
        {
            Message = message;
            this.shared = shared;

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SENTAT", new Suffix<kOS.Suffixed.TimeSpan>(() => Message.SentAt));
            AddSuffix("RECEIVEDAT", new Suffix<kOS.Suffixed.TimeSpan>(() => Message.ReceivedAt));
            AddSuffix("SENDER", new Suffix<VesselTarget>(() => Message.Sender));
            AddSuffix("CONTENT", new Suffix<object>(DeserializeContent));
        }

        public object DeserializeContent()
        {
            if (Message.Content is Dump)
            {
                return new SerializationMgr(shared).CreateFromDump(Message.Content as Dump);
            }

            return Message.Content;
        }

        public override string ToString()
        {
            return Message.ToString();
        }
    }
}

