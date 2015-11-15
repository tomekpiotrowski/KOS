using System;
using kOS.Safe.Persistence;
using System.Collections.Generic;

namespace kOS.Safe
{
    public abstract class VolumeDirectory : VolumeItem
    {
        public VolumeDirectory(Volume volume, VolumePath path) : base(volume, path)
        {

        }

        public abstract List<VolumeItem> List();
    }
}

