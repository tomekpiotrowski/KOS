using kOS.Safe.Utilities;
using System.Collections.Generic;
using System;

namespace kOS.Safe.Persistence
{
    public class Harddisk : Volume
    {
        protected const int BASE_CAPACITY = 10000;

        private int capacity;

        public HarddiskDirectory RootHarddiskDirectory { get; set; }

        public override VolumeDirectory Root { 
            get
            {
                return RootHarddiskDirectory;
            }
        }

        public Harddisk(int size)
        {
            InitializeName("");
            capacity = size;
            RootHarddiskDirectory = new HarddiskDirectory(this, VolumePath.EMPTY);
        }

        private HarddiskDirectory ParentDirectoryForPath(VolumePath path, bool create = false)
        {
            HarddiskDirectory directory = RootHarddiskDirectory;
            if (path.Depth > 1)
            {
                directory = RootHarddiskDirectory.GetSubdirectory(path.GetParent(), create);
            }

            return directory;
        }

        public override VolumeDirectory CreateDirectory(VolumePath path)
        {
            HarddiskDirectory directory = ParentDirectoryForPath(path, true);

            return directory.CreateDirectory(path.Name);
        }
            
        public override VolumeFile CreateFile(VolumePath path)
        {
            HarddiskDirectory directory = ParentDirectoryForPath(path, true);

            return directory.CreateFile(path.Name);
        }
                        
        public override VolumeItem Get(VolumePath path)
        {
            HarddiskDirectory directory = ParentDirectoryForPath(path);

            return directory.Get(path.Name);
        }

        public void Delete(VolumePath path)
        {
            HarddiskDirectory directory = ParentDirectoryForPath(path);

            directory.DeleteItem(path.Name);
        }

        public override int Capacity {
            get {
                return capacity;
            }
        }

        public override bool Renameable {
            get {
                throw new NotImplementedException ();
            }
        }

        public override float RequiredPower {
            get {
                throw new NotImplementedException ();
            }
        }


        /*

        public override bool SaveFile(VolumeFile file)
        {
            SafeHouse.Logger.Log("HardDisk: SaveFile: " + file.Name);
            return IsRoomFor(file) && base.SaveFile(file);
        }

        public override int GetFreeSpace()
        {
            return System.Math.Max(Capacity - GetUsedSpace(), 0);
        }

        public override bool IsRoomFor(VolumeFile newFile)
        {
            int usedSpace = GetUsedSpace();
            VolumeFile existingFile = GetByName(newFile.Name);

            if (existingFile != null)
            {
                usedSpace -= existingFile.GetSize();
            }

            return ((Capacity - usedSpace) >= newFile.GetSize());
        }
        */
    }
}
