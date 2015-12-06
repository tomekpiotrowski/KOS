﻿using System;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Suffixed;

namespace kOS.Communication
{
    /**
     * Any objects needing SharedObjects that are deserialized as a result of receiving messages must contain the proper
     * instance of SharedObjects. Every time a CPU requests access to a MessageQueue an instance of MessageQueueStructure
     * is created that uses this processor's SharedObjects and acts as a proxy to MessageQueue.
     */
    public class MessageQueueStructure : Structure
    {
        private MessageQueue messageQueue;
        private SharedObjects sharedObjects;

        public MessageQueueStructure(MessageQueue messageQueue, SharedObjects sharedObjects)
        {
            this.messageQueue = messageQueue;
            this.sharedObjects = sharedObjects;

            InitializeSuffixes();
        }

        public void Push(object content, kOS.Suffixed.TimeSpan sentAt, kOS.Suffixed.TimeSpan receivedAt, VesselTarget sender)
        {
            messageQueue.Push(content, sentAt, receivedAt, sender);
        }

        public void InitializeSuffixes()
        {
            AddSuffix("EMPTY",    new NoArgsSuffix<bool>                    (() => messageQueue.ReceivedCount() == 0));
            AddSuffix("LENGTH",   new NoArgsSuffix<int>                     (() => messageQueue.ReceivedCount()));
            AddSuffix("POP",      new NoArgsSuffix<MessageStructure>        (() => new MessageStructure(messageQueue.Pop(), sharedObjects)));
            AddSuffix("PEEK",     new NoArgsSuffix<MessageStructure>        (() => new MessageStructure(messageQueue.Peek(), sharedObjects)));
            AddSuffix("CLEAR",    new NoArgsSuffix                          (() => messageQueue.Clear()));
            AddSuffix("PUSH",     new OneArgsSuffix<object>                 ((m) => PushMessage(m)));
        }

        public void PushMessage(object content)
        {
            if (content is MessageStructure)
            {
                MessageStructure m = content as MessageStructure;
                messageQueue.Push(m.Message.Content, m.Message.SentAt, m.Message.ReceivedAt, m.Message.Sender);
            } else
            {
                kOS.Suffixed.TimeSpan sentAt = new kOS.Suffixed.TimeSpan(Planetarium.GetUniversalTime());
                messageQueue.Push(content, sentAt, sentAt, new VesselTarget(sharedObjects.Vessel, sharedObjects));
            }
        }

        public override string ToString()
        {
            return messageQueue.ToString();
        }
    }
}

