using System;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Communication;
using UnityEngine;
using kOS.Safe.Exceptions;

namespace kOS.Suffixed
{
    public class Connection : Structure
    {
        private SharedObjects shared;
        private Vessel vessel;

        public Connection(Vessel vessel, SharedObjects shared)
        {
            this.shared = shared;
            this.vessel = vessel;

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("ISCONNECTED", new Suffix<bool>(() => shared.ConnectivityMgr.GetDelay(shared.Vessel, vessel) != -1));
            AddSuffix("DELAY", new Suffix<double>(() => shared.ConnectivityMgr.GetDelay(shared.Vessel, vessel)));
            AddSuffix("SENDMESSAGE", new OneArgsSuffix<bool, object>(SendMessage));
        }

        public override string ToString()
        {
            return "CONNECTION(" + shared.Vessel.vesselName + ", " + vessel.vesselName + ")";
        }

        private bool SendMessage(object content)
        {
            MessageQueueStructure queue = InterVesselManager.Instance.GetQueue(vessel, shared);
            double delay = shared.ConnectivityMgr.GetDelay(shared.Vessel, vessel);

            if (delay == -1)
            {
                return false;
            }

            TimeSpan sentAt = new TimeSpan(Planetarium.GetUniversalTime());
            TimeSpan receivedAt = new TimeSpan(sentAt.ToUnixStyleTime() + delay);
            queue.Push(content, sentAt, receivedAt, new VesselTarget(shared));

            return true;
        }

    }
}