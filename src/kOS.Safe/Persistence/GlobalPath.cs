using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using kOS.Safe.Persistence;
using kOS.Safe.Exceptions;
using System.Linq;

namespace kOS.Safe.Persistence
{
    public class GlobalPath : VolumePath
    {
        private const string CurrentDirectoryPath = ".";
        public const string VolumeIdentifierRegexString = @"\A(?<id>[\w\.]+):(?<rest>.*)\Z";
        private static Regex volumeIdentifierRegex = new Regex(VolumeIdentifierRegexString);

        public static new GlobalPath EMPTY = new GlobalPath("$$empty$$", VolumePath.EMPTY);

        public object VolumeId { get; private set; }

        private GlobalPath(object volumeId, VolumePath path) : this(volumeId, new List<string>(path.Segments))
        {
        }

        private GlobalPath(object volumeId, List<string> segments) : base(new List<string>(segments))
        {
            VolumeId = ValidateVolumeId(volumeId);
        }

        private static object ValidateVolumeId(object volumeId)
        {
            if (!(volumeId is int || volumeId is string) || (volumeId is string && String.IsNullOrEmpty(volumeId as string)))
            {
                throw new KOSException("Invalid volumeId: '" + volumeId + "'");
            }

            int result;
            if (volumeId is string && int.TryParse(volumeId as string, out result))
            {
                volumeId = result;
            }

            return volumeId;
        }

        public static bool HasVolumeId(string pathString)
        {
            return volumeIdentifierRegex.Match(pathString).Success;
        }

        public new GlobalPath GetParent()
        {
            if (Depth < 1)
            {
                throw new KOSException("This path does not have a parent");
            }

            return new GlobalPath(VolumeId, new List<string>(Segments.Take(Segments.Count - 1)));
        }

        public bool IsParent(GlobalPath path)
        {
            return VolumeId.Equals(path.VolumeId) && base.IsParent(path);
        }

        public static GlobalPath FromVolumePath(VolumePath volumePath, Volume volume)
        {
            return new GlobalPath(volume.Name, new List<string>(volumePath.Segments));
        }

        public static GlobalPath FromStringAndBase(string pathString, GlobalPath basePath)
        {
            if (IsAbsolute(pathString))
            {
                throw new KOSInvalidPathException("Relative path expected", pathString);
            }

            if (pathString.Equals(CurrentDirectoryPath))
            {
                return basePath;
            }

            List<string> mergedSegments = new List<string>();
            mergedSegments.AddRange(basePath.Segments);
            mergedSegments.AddRange(GetSegmentsFromString(pathString));

            return new GlobalPath(basePath.VolumeId, mergedSegments);
        }

        public static new GlobalPath FromString(string pathString)
        {
            string volumeName = null;
            Match match = volumeIdentifierRegex.Match(pathString);

            if (match.Success) {
                volumeName = match.Groups["id"].Captures[0].Value;
                pathString = match.Groups["rest"].Captures[0].Value;
            } else {
                throw new KOSInvalidPathException("GlobalPath should contain a volumeId", pathString);
            }

            if (!pathString.StartsWith(PathSeparator.ToString()))
            {
                pathString = PathSeparator + pathString;
            }

            VolumePath path = VolumePath.FromString(pathString);
            return new GlobalPath(volumeName, path);
        }

        public override int GetHashCode()
        {
            return 13 * VolumeId.GetHashCode() + base.GetHashCode();
        }

        public override bool Equals(object other)
        {
            GlobalPath otherPath = other as GlobalPath;

            if (otherPath == null)
            {
                return false;
            }

            return VolumeId.Equals(otherPath.VolumeId) && Segments.SequenceEqual(otherPath.Segments);
        }

        public override string ToString()
        {
            return VolumeId + ":" + base.ToString();
        }
    }
}

