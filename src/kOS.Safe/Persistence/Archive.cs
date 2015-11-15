using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileInfo = kOS.Safe.Encapsulation.FileInfo;
using kOS.Safe.Compilation;
using kOS.Safe.Exceptions;

namespace kOS.Safe.Persistence
{
    public class Archive : Volume
    {
        private static string ArchiveFolder { get; set; }

        public ArchiveDirectory RootArchiveDirectory { get; private set; }

        public override VolumeDirectory Root {
            get {
                return RootArchiveDirectory;
            }
        }

        public Archive(string archiveFolder)
        {
            ArchiveFolder = archiveFolder;
            CreateArchiveDirectory();
            InitializeName("Archive");

            RootArchiveDirectory = new ArchiveDirectory(this, VolumePath.EMPTY);
        }

        public void CreateArchiveDirectory()
        {
            Directory.CreateDirectory(ArchiveFolder);
        }
            
        public string GetArchivePath(VolumePath path)
        {
            if (path.PointsOutside)
            {
                throw new KOSInvalidPathException("Path refers to parent directory", path.ToString());
            }

            string mergedPath = ArchiveFolder;

            foreach (string segment in path.Segments)
            {
                mergedPath = Path.Combine(mergedPath, segment);
            }

            return mergedPath;
        }

        public override VolumeDirectory CreateDirectory(VolumePath path)
        {
            string archivePath = GetArchivePath(path);

            try
            {
                Directory.CreateDirectory(archivePath);
            } catch (IOException)
            {
                throw new KOSPersistenceException("Already exists: " + path);
            }

            return new ArchiveDirectory(this, path);
        }
            
        public override VolumeFile CreateFile(VolumePath path)
        {
            string archivePath = GetArchivePath(path);

            Directory.CreateDirectory(GetArchivePath(path.GetParent()));

            try {
                File.Create(archivePath).Dispose();
            } catch (UnauthorizedAccessException)
            {
                throw new KOSPersistenceException("Could not create file: " + path);
            }

            return new ArchiveFile(this, path);
        }

        public override VolumeItem Get(VolumePath path)
        {
            string archivePath = GetArchivePath(path);

            // try opening as a directory first
            try {
                FileAttributes attr = File.GetAttributes(archivePath);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return new ArchiveDirectory(this, path);
                }
            } catch (FileNotFoundException) {
            }

            try
            {
                SafeHouse.Logger.Log("Archive: Getting File By Path: " + path);
                var fileInfo = FileInfoFor(archivePath);
                if (fileInfo == null)
                {
                    return null;
                }

                return new ArchiveFile(this, path);

            }
            catch (Exception e)
            {
                SafeHouse.Logger.Log(e);
                return null;
            }
        }

        public override int Capacity {
            get {
                return Volume.INFINITE_CAPACITY;
            }
        }

        public override bool Renameable {
            get {
                return false;
            }
        }

        public override float RequiredPower {
            get {
                const int MULTIPLIER = 5;
                const float POWER_REQUIRED = Volume.BASE_POWER * MULTIPLIER;

                return POWER_REQUIRED;
            }
        }

        /// <summary>
        /// Get the file from the OS.
        /// </summary>
        /// <param name="name">filename to look for</param>
        /// <returns>the full fileinfo of the filename if found</returns>
        private System.IO.FileInfo FileInfoFor(string pathString)
        {
            return File.Exists(pathString) ? new System.IO.FileInfo(pathString) : null;
        }
            

        /*

        /// <summary>
        /// Get a file given its name
        /// </summary>
        /// <param name="name">filename to get.  if it has no filename extension, one will be guessed at, ".ks" usually.</param>
        /// <param name="ksmDefault">true if a filename of .ksm is preferred in contexts where the extension was left off.  The default is to prefer .ks</param>
        /// <returns>the file</returns>
        public override VolumeFile GetByPath(VolumePath path, bool ksmDefault = false)
        {
            try
            {
                SafeHouse.Logger.Log("Archive: Getting File By Path: " + path);
                var fileInfo = FileSearch(name, ksmDefault);
                if (fileInfo == null)
                {
                    return null;
                }

                using (var infile = new BinaryReader(File.Open(fileInfo.FullName, FileMode.Open)))
                {
                    byte[] fileBody = ProcessBinaryReader(infile);

                    var retFile = new VolumeFile(fileInfo.Name);
                    FileCategory whatKind = PersistenceUtilities.IdentifyCategory(fileBody);
                    if (whatKind == FileCategory.KSM)
                        retFile.BinaryContent = fileBody;
                    else
                        retFile.StringContent = System.Text.Encoding.UTF8.GetString(fileBody);

                    if (retFile.Category == FileCategory.ASCII || retFile.Category == FileCategory.KERBOSCRIPT)
                        retFile.StringContent = retFile.StringContent.Replace("\r\n", "\n");

                    base.Add(retFile, true);

                    return retFile;
                }
            }
            catch (Exception e)
            {
                SafeHouse.Logger.Log(e);
                return null;
            }
        }

        public override bool SaveFile(VolumeFile file)
        {
            base.SaveFile(file);

            Directory.CreateDirectory(ArchiveFolder);

            try
            {
                SafeHouse.Logger.Log("Archive: Saving File Name: " + file.Name);
                byte[] fileBody;
                string fileExtension;
                switch (file.Category)
                {
                    case FileCategory.OTHER:
                    case FileCategory.TOOSHORT:
                    case FileCategory.ASCII:
                    case FileCategory.KERBOSCRIPT:
                        string tempString = file.StringContent;
                        if (SafeHouse.IsWindows)
                        {
                            // Only evil windows gets evil windows line breaks, and only if this is some sort of ASCII:
                            tempString = tempString.Replace("\n", "\r\n");
                        }
                        fileBody = System.Text.Encoding.UTF8.GetBytes(tempString.ToCharArray());
                        fileExtension = KERBOSCRIPT_EXTENSION;
                        break;

                    case FileCategory.KSM:
                        fileBody = file.BinaryContent;
                        fileExtension = KOS_MACHINELANGUAGE_EXTENSION;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var fileName = string.Format("{0}{1}", ArchiveFolder, PersistenceUtilities.CookedFilename(file.Name, fileExtension, true));
                using (var outfile = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    outfile.Write(fileBody);
                }
            }
            catch (Exception e)
            {
                SafeHouse.Logger.Log(e);
                return false;
            }

            return true;
        }

        public override bool DeleteByPath(VolumePath path)
        {
            try
            {
                SafeHouse.Logger.Log("Archive: Deleting File Name: " + name);
                base.DeleteByName(name);

                var fullPath = FileSearch(name);
                if (fullPath == null)
                {
                    return false;
                }
                File.Delete(fullPath.FullName);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool RenameFile(string name, string newName)
        {
            try
            {
                SafeHouse.Logger.Log(string.Format("Archive: Renaming: {0} To: {1}", name, newName));
                var fullSourcePath = FileSearch(name);
                if (fullSourcePath == null)
                {
                    return false;
                }

                string destinationPath = string.Format(ArchiveFolder + newName);
                if (!Path.HasExtension(newName))
                {
                    destinationPath += fullSourcePath.Extension;
                }

                File.Move(fullSourcePath.FullName, destinationPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override List<FileInfo> GetFileList()
        {
            var retList = new List<FileInfo>();

            try
            {
                SafeHouse.Logger.Log(string.Format("Archive: Listing Files"));
                var listFiles = Directory.GetFiles(ArchiveFolder);
                var filterHid = listFiles.Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) != 0);
                var filterSys = listFiles.Where(f => (File.GetAttributes(f) & FileAttributes.System) != 0);

                var visFiles = listFiles.Except(filterSys).Except(filterHid);
                var kosFiles = visFiles.Except(Directory.GetFiles(ArchiveFolder, ".*"));
                
                retList.AddRange(kosFiles.Select(file => new System.IO.FileInfo(file)).Select(sysFileInfo => new FileInfo(sysFileInfo)));
            }
            catch (DirectoryNotFoundException)
            {
            }

            return retList;
        }

        public override float RequiredPower()
        {
            const int MULTIPLIER = 5;
            const float POWER_REQUIRED = BASE_POWER * MULTIPLIER;

            return POWER_REQUIRED;
        }



       



        public override bool AppendToFile(string name, byte[] bytesToAppend)
        {
            SafeHouse.Logger.SuperVerbose("Archive: AppendToFile: " + name);
            System.IO.FileInfo info = FileSearch(name);

            string fullPath = info == null ? string.Format("{0}{1}", ArchiveFolder, PersistenceUtilities.CookedFilename(name, KERBOSCRIPT_EXTENSION, true)) : info.FullName;

            // Deliberately not catching potential I/O exceptions from this, so they will percolate upward and be seen by the user:
            using (var outfile = new BinaryWriter(File.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                outfile.Write(bytesToAppend);
            }
            return true;
        }
        */

    }
}