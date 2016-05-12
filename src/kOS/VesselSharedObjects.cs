using System;
using kOS.Execution;

namespace kOS
{
    public class VesselSharedObjects : SharedObjects
    {
        public Vessel Vessel { get; set; }
        public Part KSPPart { get; set; }
        public TransferManager TransferManager { get; set; }

        public VesselSharedObjects(Vessel vessel)
        {
            Vessel = vessel;

            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
        }

        private void OnVesselDestroy(Vessel data)
        {
            if (data.id == Vessel.id)
            {
                BindingMgr.Dispose();
            }
        }
    }
}

