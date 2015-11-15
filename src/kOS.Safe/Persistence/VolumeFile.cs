using System;
using System.Linq;
using System.Text;
using kOS.Safe.Exceptions;
using kOS.Safe.Compilation;
using System.Collections.Generic;

namespace kOS.Safe.Persistence
{
    public abstract class VolumeFile : VolumeItem
    {
        public FileCategory Category
        {
            get
            {
                return PersistenceUtilities.IdentifyCategory(Read());
            }
        }

        public VolumeFile(Volume volume, VolumePath path) : base(volume, path)
        {
        }

        public abstract byte[] Read();

        public string ReadAsString()
        {
            byte[] bytes = Read();

            FileCategory fileCategory = PersistenceUtilities.IdentifyCategory(bytes);

            if (fileCategory == FileCategory.KSM)
            {
                throw new KOSException("File contains binary data");
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public abstract bool Write(byte[] content);

        public bool Write(String content)
        {
            return Write(Encoding.UTF8.GetBytes(content));
        }

        public bool Write(List<CodePart> parts) {
            throw new NotImplementedException();
        }

        public abstract bool Append(byte[] content);

    }
}
