using System;
using kOS.Communication;
using System.Collections.Generic;
using kOS.Safe.Serialization;
using kOS.Safe.Encapsulation;
using kOS.Safe.Exceptions;
using UnityEngine;
using System.Linq;
using kOS.Serialization;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe;
using kOS.Suffixed;

namespace kOS.Communication
{
    public class MessageQueue : Structure, IDumper
    {
        private List<Message> messages = new List<Message>();

        private List<Message> ReceivedMessages {
            get {
                return messages.Where(IsReceived).ToList();
            }
        }

        private bool IsReceived(Message message)
        {
            return message.ReceivedAt.ToUnixStyleTime() <= Planetarium.GetUniversalTime();
        }

        public void Clear()
        {
            messages.RemoveAll(IsReceived);
        }

        public Message Peek()
        {
            if (messages.Count > 0 && IsReceived(messages[0]))
            {
                var m = messages[0];

                return m;
            }

            throw new KOSException("Message queue is empty");
        }

        public Message Pop()
        {
            if (messages.Count > 0 && IsReceived(messages[0]))
            {
                Message m =  messages[0];
                messages.RemoveAt(0);

                return m;
            }

            throw new KOSException("Message queue is empty");
        }

        public int Count()
        {
            return messages.Count;
        }

        public int ReceivedCount()
        {
            return ReceivedMessages.Count();
        }

        public void Push(object content, kOS.Suffixed.TimeSpan sentAt, kOS.Suffixed.TimeSpan receivedAt, VesselTarget sender)
        {
            Message message = new Message();
            message.SentAt = sentAt;
            message.ReceivedAt = receivedAt;
            message.Sender = sender;

            if (content is IDumper)
            {
                message.Content = new SafeSerializationMgr().Dump(content as IDumper, true);
            } else if (SerializationMgr.IsValue(content))
            {
                message.Content = content;
            } else
            {
                throw new KOSException("Only serializable types and primitives can be sent in a message");
            }

            InsertMessage(message);
        }

        private void InsertMessage(Message message)
        {
            messages.Add(message);

            /**
             * It would be better if this was a proper priority queue, but C# has no default implemention. Anyway, I can't imagine
             * a scenario in kOS where this could actually impact performance.
            */
            messages.Sort();
        }

        public override string ToString()
        {
            return "MESSAGE QUEUE";
        }

        public Dump Dump()
        {
            DumpWithHeader dump = new DumpWithHeader();
            dump.Header = "MESSAGE QUEUE";

            int i = 0;

            foreach (Message message in messages)
            {
                dump.Add(i, message);

                i++;
            }

            return dump;
        }

        public void LoadDump(Dump dump)
        {
            messages.Clear();

            foreach (KeyValuePair<object, object> entry in dump)
            {
                Message message = entry.Value as Message;

                if (message != null)
                {
                    messages.Add(message);
                }
            }

            messages.Sort();
        }

    }
}

