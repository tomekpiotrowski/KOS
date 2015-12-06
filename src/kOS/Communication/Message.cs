using System;
using kOS.Safe.Encapsulation;
using kOS.Suffixed;
using kOS.Safe.Serialization;
using kOS.Safe.Exceptions;
using kOS.Safe;

namespace kOS.Communication
{
    public class Message : IDumper, IComparable<Message>
    {
        public const string DUMP_SENT_AT = "sentAt";
        public const string DUMP_RECEIVED_AT = "receivedAt";
        public const string DUMP_SENDER = "sender";
        public const string DUMP_CONTENT = "content";

        public kOS.Suffixed.TimeSpan SentAt { get; set; }
        public kOS.Suffixed.TimeSpan ReceivedAt { get; set; }
        public VesselTarget Sender { get; set; }
        private object content;

        public object Content {
            get {
                return content;
            }
            set {
                if (SafeSerializationMgr.IsValue(value) || value is Dump)
                {
                    content = value;
                } else
                {
                    throw new KOSException("Message can only contain primitives and serializable types");
                }
            }
        }

        public Message()
        {
        }

        public Dump Dump()
        {
            DumpWithHeader dump = new DumpWithHeader();

            dump.Header = "MESSAGE";

            dump.Add(DUMP_SENT_AT, SentAt);
            dump.Add(DUMP_RECEIVED_AT, ReceivedAt);
            dump.Add(DUMP_SENDER, Sender);
            dump.Add(DUMP_CONTENT, content);

            return dump;
        }

        public void LoadDump(Dump dump)
        {
            SentAt = dump[DUMP_SENT_AT] as kOS.Suffixed.TimeSpan;
            ReceivedAt = dump[DUMP_RECEIVED_AT] as kOS.Suffixed.TimeSpan;
            Sender = dump[DUMP_SENDER] as VesselTarget;
            content = dump[DUMP_CONTENT];
        }

        public override string ToString()
        {
            return "MESSAGE FROM " + Sender.GetName();
        }

        public int CompareTo(Message other)
        {
            return ReceivedAt.CompareTo(other.ReceivedAt);
        }
    }
}

