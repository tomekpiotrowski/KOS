﻿using System;
using kOS.Safe.Serialization;
using kOS.Safe.Persistence;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Encapsulation;
using System.Linq;

namespace kOS.Safe
{
    /// <summary>
    /// Contains suffixes related to GlobalPath.
    ///
    /// This exists as a separate class because some of the suffixes require an instance of VolumeManager to work. I think
    /// it would be counter-productive to pass around an instance of VolumeManager whenever we're dealing with GlobalPath internally.
    /// Instances of this class are on the other hand created only for the user.
    /// </summary>
    [kOS.Safe.Utilities.KOSNomenclature("Path")]
    public class PathValue : SerializableStructure, IHasSafeSharedObjects
    {
        private const string DumpPath = "path";

        public GlobalPath Path { get; private set; }
        private SafeSharedObjects sharedObjects;

        public SafeSharedObjects Shared {
            set {
                sharedObjects = value;
            }
        }

        public PathValue(GlobalPath path, SafeSharedObjects sharedObjects)
        {
            Path = path;
            this.sharedObjects = sharedObjects;

            InitializeSuffixes();
        }

        public PathValue FromPath(GlobalPath path)
        {
            return new PathValue(path, sharedObjects);
        }

        public PathValue FromPath(VolumePath volumePath, Volume volume)
        {
            return new PathValue(GlobalPath.FromVolumePath(volumePath, volume), sharedObjects);
        }

        private void InitializeSuffixes()
        {
            AddSuffix("VOLUME", new Suffix<Volume>(() => sharedObjects.VolumeMgr.GetVolumeFromPath(Path)));
            AddSuffix("PARENT", new Suffix<PathValue>(() => FromPath(Path.GetParent())));
            AddSuffix("ISPARENT", new OneArgsSuffix<BooleanValue, PathValue>((p) => Path.IsParent(p.Path)));
            AddSuffix("ROOT", new Suffix<PathValue>(() => FromPath(Path.RootPath())));
            AddSuffix("SEGMENTS", new Suffix<ListValue>(() => new ListValue(Path.Segments.Select((s) => (Structure)new StringValue(s)))));
            AddSuffix("LENGTH", new Suffix<ScalarIntValue>(() => Path.Length));
            AddSuffix("DEPTH", new Suffix<ScalarIntValue>(() => Path.Depth));
            AddSuffix("NAME", new Suffix<StringValue>(() => Path.Name));
            AddSuffix("HASEXTENSION", new Suffix<BooleanValue>(() => string.IsNullOrEmpty(Path.Extension)));
            AddSuffix("EXTENSION", new Suffix<StringValue>(() => Path.Extension));
            AddSuffix("CHANGEEXTENSION", new OneArgsSuffix<PathValue, StringValue>((e) => FromPath(Path.ChangeExtension(e))));
            AddSuffix("COMBINE", new VarArgsSuffix<PathValue, StringValue>((segments) => FromPath(Path.Combine(segments.Cast<string>().ToArray()))));
        }

        public override Dump Dump()
        {
            return new Dump { { DumpPath, Path.ToString() } };
        }

        public override void LoadDump(Dump dump)
        {
            Path = GlobalPath.FromString(dump[DumpPath] as string);
        }

        public override string ToString()
        {
            return Path.ToString();
        }
    }
}

