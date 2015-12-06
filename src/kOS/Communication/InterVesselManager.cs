using System;
using System.Collections.Generic;
using kOS.Safe.Encapsulation;
using kOS.Safe.Serialization;
using kOS.Serialization;
using kOS.Safe;

namespace kOS.Communication
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames | ScenarioCreationOptions.AddToExistingGames, GameScenes.FLIGHT, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    public class InterVesselManager : ScenarioModule
    {
        private const string ID = "id";
        private static string VESSEL_QUEUE = "vesselQueue";
        private static string MESSAGE_QUEUE = "messageQueue";

        private Dictionary<string, MessageQueue> vesselQueues;

        public static InterVesselManager Instance { get; private set; }

        static InterVesselManager() {
            // normally we do this in SerializationMgr, but KSPScenarios run before we create any instances
            SafeSerializationMgr.AddAssembly(typeof(SerializationMgr).Assembly.FullName);
        }

        public InterVesselManager()
        {
        }

        public override void OnLoad(ConfigNode node)
        {
            Instance = this;
            vesselQueues = new Dictionary<string, MessageQueue>();

            foreach (ConfigNode subNode in node.GetNodes())
            {
                if (subNode.name.Equals(VESSEL_QUEUE))
                {
                    string id = subNode.GetValue(ID);

                    ConfigNode queueNode = subNode.GetNode(MESSAGE_QUEUE);

                    Dump queueDump = ConfigNodeFormatter.Instance.FromConfigNode(queueNode);

                    MessageQueue queue = new SafeSerializationMgr().CreateFromDump(queueDump) as MessageQueue;

                    if (queue.Count() > 0)
                    {
                        vesselQueues[id] = queue;
                    }
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vesselQueues.ContainsKey(vessel.id.ToString()))
                {
                    string id = vessel.id.ToString();
                    ConfigNode vesselEntry = new ConfigNode(VESSEL_QUEUE);
                    vesselEntry.AddValue(ID, id);

                    ConfigNode queueNode = ConfigNodeFormatter.Instance.ToConfigNode(new SafeSerializationMgr().Dump(vesselQueues[id]));
                    queueNode.name = MESSAGE_QUEUE;
                    vesselEntry.AddNode(queueNode);

                    node.AddNode(vesselEntry);
                }
            }
        }

        public MessageQueueStructure GetQueue(Vessel vessel, SharedObjects sharedObjects)
        {
            string vesselId = vessel.id.ToString();

            if (!vesselQueues.ContainsKey(vesselId))
            {
                vesselQueues.Add(vesselId, new MessageQueue());
            }

            return new MessageQueueStructure(vesselQueues[vesselId], sharedObjects);
        }
    }
}

