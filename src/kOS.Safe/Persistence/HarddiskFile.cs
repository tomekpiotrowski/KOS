using System;
using kOS.Safe.Persistence;
using System.Text;

namespace kOS.Safe.Persistence
{
    public class HarddiskFile : VolumeFile
    {
        public byte[] Content { get; set; }
        private Harddisk harddisk;


        public HarddiskFile(Harddisk harddisk, VolumePath path) : base(harddisk, path)
        {
            this.harddisk = harddisk;

            Content = new byte[0];
        }

        public override void Move(VolumePath newPath)
        {
            throw new NotImplementedException();
        }
        public override void Delete()
        {
            harddisk.Delete(Path);
        }

        public override int Size { get { return Read().Length; } }

        public override byte[] Read()
        {
            return Content;
        }

        public override bool Write(byte[] content)
        {
            Content = content;

            return true;
        }

        public override bool Append(byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}

