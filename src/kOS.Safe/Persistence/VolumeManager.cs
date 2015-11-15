using System;
using System.Collections.Generic;
using kOS.Safe.Exceptions;

namespace kOS.Safe.Persistence
{
    public class VolumeManager
    {
        private readonly Dictionary<int, Volume> volumes;
        private int lastId;

        public virtual Volume CurrentVolume { get; set; }
        public Dictionary<int, Volume> Volumes { get { return volumes; } }
        public VolumeDirectory CurrentDirectory { get; set; }
        public float CurrentRequiredPower { get; private set; }

        public VolumeManager()
        {
            volumes = new Dictionary<int, Volume>();
            CurrentVolume = null;
            CurrentDirectory = null;
        }


        public bool VolumeIsCurrent(Volume volume)
        {
            return volume == CurrentVolume;
        }

        private int GetVolumeId(string name)
        {
            int volumeId = -1;
            
            foreach (KeyValuePair<int, Volume> kvp in volumes)
            {
                if (String.Equals(kvp.Value.Name, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    volumeId = kvp.Key;
                    break;
                }
            }

            return volumeId;
        }

        public int GetVolumeId(Volume volume)
        {
            int volumeId = -1;

            foreach (KeyValuePair<int, Volume> kvp in volumes)
            {
                if (kvp.Value == volume)
                {
                    volumeId = kvp.Key;
                    break;
                }
            }

            return volumeId;
        }

        public Volume GetVolume(object volumeId)
        {
            if (volumeId is int || volumeId is double || volumeId is float)
            {
                return GetVolume((int)volumeId);
            }
            return GetVolume(volumeId.ToString());
        }

        public Volume GetVolume(string name)
        {
            return GetVolume(GetVolumeId(name));
        }

        public virtual Volume GetVolume(int id)
        {
            if (volumes.ContainsKey(id))
            {
                return volumes[id];
            }
            return null;
        }

        public void Add(Volume volume)
        {
            if (!volumes.ContainsValue(volume))
            {
                volumes.Add(lastId++, volume);

                if (CurrentVolume == null)
                {
                    CurrentVolume = volumes[0];
                    UpdateRequiredPower();
                }
            }
        }

        public void Remove(string name)
        {
            Remove(GetVolumeId(name));
        }

        public void Remove(int id)
        {
            Volume volume = GetVolume(id);

            if (volume != null)
            {
                volumes.Remove(id);

                if (CurrentVolume == volume)
                {
                    if (volumes.Count > 0)
                    {
                        CurrentVolume = volumes[0];
                        UpdateRequiredPower();
                    }
                    else
                    {
                        CurrentVolume = null;
                    }
                }
            }
        }

        public void SwitchTo(Volume volume)
        {
            CurrentVolume = volume;
            CurrentDirectory = volume.Root;
            UpdateRequiredPower();
        }

        public void UpdateVolumes(List<Volume> attachedVolumes)
        {
            // Remove volumes that are no longer attached
            var removals = new List<int>();
            foreach (var kvp in volumes)
            {
                if (!(kvp.Value is Archive) && !attachedVolumes.Contains(kvp.Value))
                {
                    removals.Add(kvp.Key);
                }
            }

            foreach (int id in removals)
            {
                volumes.Remove(id);
            }

            // Add volumes that have become attached
            foreach (Volume volume in attachedVolumes)
            {
                if (volume != null && !volumes.ContainsValue(volume))
                {
                    Add(volume);
                }
            }
        }

        public string GetVolumeBestIdentifier(Volume volume)
        {
            int id = GetVolumeId(volume);
            if (!string.IsNullOrEmpty(volume.Name)) return string.Format("#{0}: \"{1}\"", id, volume.Name);
            return "#" + id;
        }
        
        /// <summary>
        /// Like GetVolumeBestIdentifier, but without the extra string formatting.
        /// </summary>
        /// <param name="volume"></param>
        /// <returns>The Volume's Identifier without pretty formatting</returns>
        public string GetVolumeRawIdentifier(Volume volume)
        {
            int id = GetVolumeId(volume);
            return !string.IsNullOrEmpty(volume.Name) ? volume.Name : id.ToString();
        }

        private void UpdateRequiredPower()
        {
            CurrentRequiredPower = (float)Math.Round(CurrentVolume.RequiredPower, 4);
        }

        // Handles global, absolute and relative paths
        public GlobalPath GlobalPathFromString(string pathString)
        {
            if (GlobalPath.HasVolumeId(pathString))
            {
                return GlobalPath.FromString(pathString);
            }
            else
            {
                if (GlobalPath.IsAbsolute(pathString))
                {
                    return GlobalPath.FromVolumePath(VolumePath.FromString(pathString), CurrentVolume);
                }
                else
                {
                    return GlobalPath.FromStringAndBase(pathString, GlobalPath.FromVolumePath(CurrentDirectory.Path, CurrentVolume));
                }
            }

        }

        public Volume FromPath(GlobalPath path)
        {
            Volume volume = GetVolume(path.VolumeId);

            if (volume == null)
            {
                throw new KOSPersistenceException("Volume not found: " + path.VolumeId);
            }

            return volume;
        }
    }
}
