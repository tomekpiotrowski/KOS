using kOS.Factories;
using kOS.Safe.Persistence;
using kOS.Safe.Screen;
using kOS.Safe.Utilities;

namespace kOS.AddOns.RemoteTech
{
    public class RemoteTechFactory : IFactory
    {
        public IInterpreter CreateInterpreter(SharedObjects shared)
        {
            return new RemoteTechInterpreter(shared);
        }

        public Archive CreateArchive()
        {
            return new RemoteTechArchive(SafeHouse.ArchiveFolder);
        }

        public VolumeManager CreateVolumeManager(SharedObjects sharedObjects)
        {
            return new RemoteTechVolumeManager(sharedObjects);
        }
    }
}
