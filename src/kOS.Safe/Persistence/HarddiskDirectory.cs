using System;
using System.Collections.Generic;
using kOS.Safe.Persistence;
using System.Linq;
using kOS.Safe.Exceptions;

namespace kOS.Safe.Persistence
{
    public class HarddiskDirectory : VolumeDirectory, IEnumerable<VolumeItem>
    {
        private Dictionary<string, VolumeItem> Items { get; set; }
        private Harddisk harddisk;

        public HarddiskDirectory(Harddisk harddisk, VolumePath path) : base(harddisk, path)
        {
            this.harddisk = harddisk;
            Items = new Dictionary<string, VolumeItem>();
        }

        public VolumeItem Get(string name)
        {
            return Items[name];
        }

        public HarddiskFile CreateFile(string name)
        {
            return CreateFile(name, new HarddiskFile(Volume as Harddisk, VolumePath.FromString(name, Path)));
        }

        public HarddiskFile CreateFile(string name, HarddiskFile file)
        {
            try {
                Items.Add(name, file);
            } catch (ArgumentException)
            {
                throw new KOSPersistenceException("Already exists: " + name);
            }

            return file;
        }

        public HarddiskDirectory CreateDirectory(string name)
        {
            return CreateDirectory(name, new HarddiskDirectory(Volume as Harddisk, VolumePath.FromString(name, Path)));
        }

        public HarddiskDirectory CreateDirectory(string name, HarddiskDirectory directory)
        {
            try
            {
                Items.Add(name, directory);
            } catch (ArgumentException)
            {
                throw new KOSPersistenceException("Already exists: " + name);
            }

            return directory;
        }

        public IEnumerator<VolumeItem> GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public HarddiskDirectory GetSubdirectory(VolumePath path, bool create = false)
        {
            if (Path.Equals(path))
            {
                return this;
            }

            if (!Path.IsParent(path))
            {
                throw new KOSException("This directory does not contain that path: " + path.ToString());
            }

            string subdirectory = path.Segments[Path.Segments.Count];

            if (!Items.ContainsKey(subdirectory))
            {
                if (create)
                {
                    CreateDirectory(subdirectory);
                } else
                {
                    return null;
                }
            }

            HarddiskDirectory directory = Items[subdirectory] as HarddiskDirectory;

            if (directory == null)
            {
                throw new KOSException("Subdirectory does not exist: " + path.ToString());
            }

            return directory;
        }

        public override void Move(VolumePath newPath)
        {
            throw new NotImplementedException();
        }

        public override void Delete()
        {
            harddisk.Delete(Path);
        }

        public void DeleteItem(string name)
        {
            Items.Remove(name);
        }

        public override List<VolumeItem> List()
        {
            IEnumerable<VolumeItem> sortedItems = Items.Values.OrderBy(item => item.Name);
            List<VolumeItem> result = new List<VolumeItem>();
            List<VolumeItem> files = new List<VolumeItem>();

            foreach (VolumeItem item in sortedItems)
            {
                if (item is VolumeDirectory)
                {
                    result.Add(item);
                }
                else
                {
                    files.Add(item);
                }
            }

            result.AddRange(files);

            return result;
        }

        public override int Size {
            get {
                return Items.Aggregate(0, (acc, x) => acc + x.Value.Size);
            }
        }
    }
}

