using kOS.Safe.Binding;
using kOS.Suffixed;
using kOS.Suffixed.Part;
using kOS.Utilities;
using kOS.Module;

namespace kOS.Binding
{
    [Binding("ksp")]
    public class MissionSettings : Binding
    {
        public override void AddTo(SharedObjects shared)
        {
            shared.BindingMgr.AddGetter("CORE", () => new Core((kOSProcessorModule)shared.Processor, shared));

            shared.BindingMgr.AddSetter("TARGET", val =>
            {
                if (shared.Vessel != FlightGlobals.ActiveVessel)
                {
                    throw new kOS.Safe.Exceptions.KOSSituationallyInvalidException("TARGET can only be set for the Active Vessel");
                }
                var targetable = val as IKOSTargetable;
                if (targetable != null)
                {
                    VesselUtils.SetTarget(targetable);
                    return;
                }

                if (!string.IsNullOrEmpty(val.ToString().Trim()))
                {
                    var body = VesselUtils.GetBodyByName(val.ToString());
                    if (body != null)
                    {
                        VesselUtils.SetTarget(body);
                        return;
                    }

                    var vessel = VesselUtils.GetVesselByName(val.ToString(), shared.Vessel);
                    if (vessel != null)
                    {
                        VesselUtils.SetTarget(vessel);
                        return;
                    }
                }
                //Target not found, if we have a target we clear it
                VesselUtils.UnsetTarget();
            });

            shared.BindingMgr.AddGetter("TARGET", () =>
            {
                if (shared.Vessel != FlightGlobals.ActiveVessel)
                {
                    throw new kOS.Safe.Exceptions.KOSSituationallyInvalidException("TARGET can only be returned for the Active Vessel");
                }
                var currentTarget = FlightGlobals.fetch.VesselTarget;

                var vessel = currentTarget as Vessel;
                if (vessel != null)
                {
                    return new VesselTarget(vessel, shared);
                }
                var body = currentTarget as CelestialBody;
                if (body != null)
                {
                    return new BodyTarget(body, shared);
                }
                var dockingNode = currentTarget as ModuleDockingNode;
                if (dockingNode != null)
                {
                    return new DockingPortValue(dockingNode, shared);
                }

                throw new kOS.Safe.Exceptions.KOSSituationallyInvalidException("No TARGET is selected");
            });

            shared.BindingMgr.AddGetter("HASTARGET", () =>
            {
                if (shared.Vessel != FlightGlobals.ActiveVessel) return false;
                // the ship has a target if the object does not equal null.
                return FlightGlobals.fetch.VesselTarget != null;
            });

        }
    }
}