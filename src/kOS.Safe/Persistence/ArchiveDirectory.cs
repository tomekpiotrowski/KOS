using System;
using kOS.Safe.Persistence;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace kOS.Safe.Persistence
{
    public class ArchiveDirectory : VolumeDirectory
    {
        private Archive archive;
        private string archivePath;

        public ArchiveDirectory(Archive archive, VolumePath path) : base(archive, path)
        {
            this.archive = archive;
            this.archivePath = archive.GetArchivePath(path);
        }
            
        public override void Move(VolumePath newPath)
        {
            throw new NotImplementedException();
        }

        public override void Delete()
        {
            try {
                Directory.Delete(archivePath, true);
            } catch (DirectoryNotFoundException)
            {
            }
        }

        public override List<VolumeItem> List()
        {
            string[] files = Directory.GetFiles(archivePath);
            string[] directories = Directory.GetDirectories(archivePath);

            Array.Sort(files);
            Array.Sort(directories);

            List<VolumeItem> result = new List<VolumeItem>();

            foreach (string directory in directories)
            {
                string directoryName = System.IO.Path.GetFileName(directory);
                result.Add(new ArchiveDirectory(archive, VolumePath.FromString(directoryName, Path)));
            }

            foreach (string file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);
                result.Add(new ArchiveFile(archive, VolumePath.FromString(fileName, Path)));
            }

            return result;
        }

        public override int Size {
            get {
                return List().Aggregate(0, (acc, x) => acc + x.Size);
            }
        }
    }
}

