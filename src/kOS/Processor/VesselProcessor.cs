using kOS.Safe.Persistence;
using kOS.Safe.Processor;
using kOS.Execution;

namespace kOS.Processor
{
    public class VesselProcessor : Processor<Harddisk, VesselSharedObjects>
    {
        private readonly Vessel vessel;
        private readonly Part part;

        public VesselProcessor(Vessel vessel, Part part, ProcessorModes mode) : base(mode)
        {
            this.vessel = vessel;
            this.part = part;
        }

        public void Initialize()
        {
            base.Initialize();

            shared = new VesselSharedObjects(vessel);
            shared.KSPPart = part;
            shared.TransferManager = new TransferManager(shared);

        }
    }
}

