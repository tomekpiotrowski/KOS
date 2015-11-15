using System;
using kOS.Safe.Persistence;
using System.Linq;

namespace kOS.Safe.Persistence
{
    public abstract class VolumeItem
    {
        public Volume Volume { get; set; }
        public VolumePath Path { get; set; }

        public string Name
        {
            get
            {
                return Path.Name;
            }
        }

        public string Extension {
            get
            {
                var fileParts = Name.Split('.');
                return fileParts.Count() > 1 ? fileParts.Last() : string.Empty;
            }
        }

        public VolumeItem(Volume volume, VolumePath path)
        {
            Volume = volume;
            Path = path;
        }

        public VolumeItem(Volume volume, VolumePath parentPath, String name)
        {
            Volume = volume;
            Path = VolumePath.FromString(name, parentPath);
        }

        public override string ToString()
        {
            return Path.ToString();
        }

        public abstract int Size { get; }

        public abstract void Move(VolumePath newPath);
        public abstract void Delete();
    }
}

