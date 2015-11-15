using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Compilation;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using FileInfo = kOS.Safe.Encapsulation.FileInfo;
using kOS.Safe.Exceptions;

namespace kOS.Safe.Persistence
{
    public abstract class Volume : Structure
    {
        public const string TEXT_EXTENSION = "txt";
        public const string KERBOSCRIPT_EXTENSION = "ks";
        public const string KOS_MACHINELANGUAGE_EXTENSION = "ksm";

        public const int INFINITE_CAPACITY = -1;
        protected const float BASE_POWER = 0.04f;

        public abstract VolumeDirectory Root { get; }

        private string name;

        public string Name {
            get {
                return name;
            }
            set { 
                if (Renameable) {
                    name = value;
                } else {
                    throw new KOSException("Volume name can't be changed");
                }
            }
        }

        protected void InitializeName(string name)
        {
            this.name = name;
        }

        public abstract int Capacity { get; }
        public int FreeSpace {
            get {
                return Capacity == Volume.INFINITE_CAPACITY ? Volume.INFINITE_CAPACITY : Capacity - Root.Size;
            }
        }
        public abstract bool Renameable { get; }
        public abstract float RequiredPower { get; }

        protected Volume()
        {
            InitializeVolumeSuffixes();
        }

        public abstract VolumeItem Get(VolumePath path);
        public abstract VolumeDirectory CreateDirectory(VolumePath path);
        public abstract VolumeFile CreateFile(VolumePath path);

        public VolumeDirectory GetOrCreateDirectory(VolumePath path)
        {
            VolumeDirectory directory = Get(path) as VolumeDirectory;

            if (directory == null)
            {
                directory = CreateDirectory(path);
            }

            return directory;
        }

        public VolumeFile GetOrCreateFile(VolumePath path)
        {
            VolumeFile file = Get(path) as VolumeFile;

            if (file == null)
            {
                file = CreateFile(path);
            }

            return file;
        }

        public override string ToString()
        {
            return "Volume(" + Name + ", " + Capacity + ")";
        }

        private void InitializeVolumeSuffixes()
        {
            AddSuffix("FREESPACE" , new Suffix<float>(() => FreeSpace));
            AddSuffix("CAPACITY" , new Suffix<float>(() => Capacity));
            AddSuffix("NAME" , new SetSuffix<string>(() => Name, SetName));
            AddSuffix("RENAMEABLE" , new Suffix<bool>(() => Renameable));
            AddSuffix("FILES" , new Suffix<ListValue<VolumeItem>>(() => new ListValue<VolumeItem>(Root.List())));
            AddSuffix("POWERREQUIREMENT" , new Suffix<float>(() => RequiredPower));
        }

        private void SetName(string name)
        {
            this.Name = name;
        }

        public bool IsRoomFor(byte[] content)
        {
            return Capacity == INFINITE_CAPACITY || content.Length <= FreeSpace;
        }

        /*public abstract Dictionary<string, VolumeItem> ItemList
        {
            get
            {
                SafeHouse.Logger.SuperVerbose("Volume: Get-ItemList: " + items.Count);
                return items.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            }
        }*/
        /*


        /// <summary>
        /// Get a file given its name
        /// </summary>
        /// <param name="name">filename to get.  if it has no filename extension, one will be guessed at, ".ks" usually.</param>
        /// <param name="ksmDefault">in the scenario where there is no filename extension, do we prefer the .ksm over the .ks?  The default is to prefer .ks</param>
        /// <returns>the file</returns>
        public virtual VolumeFile GetByPath(VolumePath path, bool ksmDefault = false )
        {
            SafeHouse.Logger.SuperVerbose("Volume: GetByPath: " + path);
            var fullPath = FileSearch(name, ksmDefault);
            if (fullPath == null)
            {
                return null;
            }

            return files.ContainsKey(fullPath.Name) ? files[fullPath.Filename] : null;
        }

        public virtual bool DeleteByPath(VolumePath path)
        {
            SafeHouse.Logger.SuperVerbose("Volume: DeleteByName: " + name);

            var fullPath = FileSearch(name);
            if (fullPath == null)
            {
                return false;
            }
            if (files.ContainsKey(fullPath.Filename))
            {
                files.Remove(fullPath.Filename);
                return true;
            }
            return false;
        }

        public virtual bool RenameFile(string name, string newName)
        {
            SafeHouse.Logger.SuperVerbose("Volume: RenameFile: From: " + name + " To: " + newName);
            VolumeFile file = GetByName(name);
            if (file != null)
            {
                DeleteByName(name);
                file.Filename = newName;
                Add(file);
                return true;
            }
            return false;
        }

        public virtual bool AppendToFile(string name, string textToAppend)
        {
            SafeHouse.Logger.SuperVerbose("Volume: AppendToFile: " + name);
            VolumeFile file = GetByName(name) ?? new VolumeFile(name);

            if (file.StringContent.Length > 0 && !file.StringContent.EndsWith("\n"))
            {
                textToAppend = "\n" + textToAppend;
            }

            file.StringContent = file.StringContent + textToAppend;
            return SaveFile(file);
        }

        public virtual bool AppendToFile(string name, byte[] bytesToAppend)
        {
            SafeHouse.Logger.SuperVerbose("Volume: AppendToFile: " + name);
            VolumeFile file = GetByName(name) ?? new VolumeFile(name);

            file.BinaryContent = new byte[file.BinaryContent.Length + bytesToAppend.Length];
            Array.Copy(bytesToAppend, 0, file.BinaryContent, file.BinaryContent.Length, bytesToAppend.Length);
            return SaveFile(file);
        }

        public virtual void Add(VolumeFile file, bool withReplacement = false)
        {
            SafeHouse.Logger.SuperVerbose("Volume: Add: " + file.Filename);
            if (withReplacement)
            {
                files[file.Filename] = file;
            }
            else
            {
                files.Add(file.Filename, file);
            }
        }

        public virtual bool SaveFile(VolumeFile file)
        {
            SafeHouse.Logger.SuperVerbose("Volume: SaveFile: " + file.Filename);
            
            Add(file, true);
            return true;
        }
        
        public virtual bool SaveObjectFile(string fileNameOut, List<CodePart> parts)
        {
            var newFile = new VolumeFile(fileNameOut) {BinaryContent = CompiledObject.Pack(parts)};
            return SaveFile(newFile);
        }



        protected int GetUsedSpace()
        {
            return files.Values.Sum(file => file.GetSize());
        }

        public virtual int GetFreeSpace() { return -1; }
        public virtual bool IsRoomFor(VolumeFile newFile) { return true; }

        public virtual List<FileInfo> GetFileList()
        {
            SafeHouse.Logger.SuperVerbose("Volume: GetFileList: " + files.Count);
            List<FileInfo> returnList = files.Values.Select(file => new FileInfo(file.Filename, file.GetSize())).ToList();
            returnList.Sort(FileInfoComparer); // make sure files will print in sorted form.
            return returnList;
        }

        public virtual float RequiredPower()
        {
            var multiplier = ((float)Capacity) / BASE_CAPACITY;
            var powerRequired = BASE_POWER * multiplier;

            return powerRequired;
        }



        private VolumeFile FileSearch(string name, bool ksmDefault = false)
        {
            SafeHouse.Logger.SuperVerbose("Volume: FileSearch: " + files.Count);
            var kerboscriptFilename = PersistenceUtilities.CookedFilename(name, KERBOSCRIPT_EXTENSION, true);
            var kosMlFilename = PersistenceUtilities.CookedFilename(name, KOS_MACHINELANGUAGE_EXTENSION, true);

            VolumeFile kerboscriptFile;
            VolumeFile kosMlFile;
            bool kerboscriptFileExists = files.TryGetValue(kerboscriptFilename, out kerboscriptFile);
            bool kosMlFileExists = files.TryGetValue(kosMlFilename, out kosMlFile);
            if (kerboscriptFileExists && kosMlFileExists)
            {
                return ksmDefault ? kosMlFile : kerboscriptFile;
            }
            if (kerboscriptFile != null)
            {
                return kerboscriptFile;
            }
            if (kosMlFile != null)
            {
                return kosMlFile;
            }
            return null;
        }
        
        private int FileInfoComparer(FileInfo a, FileInfo b)
        {
            return String.CompareOrdinal(a.Name, b.Name);
        }
        */
    }
}
