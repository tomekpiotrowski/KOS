using System;
using kOS.Safe.Persistence;
using System.IO;
using kOS.Safe.Utilities;

namespace kOS.Safe.Persistence
{
    public class ArchiveFile : VolumeFile
    {
        private FileInfo fileInfo;
        private Archive archive;
        private string archivePath;

        public ArchiveFile(Archive archive, VolumePath path) : base(archive, path)
        {
            this.archive = archive;
            this.archivePath = archive.GetArchivePath(path);
        }

        public override void Move(VolumePath newPath)
        {
            throw new NotImplementedException();
        }

        public override void Delete()
        {
            File.Delete(archivePath);
        }

        public override bool Write(byte[] content)
        {
            archive.CreateArchiveDirectory();

            try
            {
                SafeHouse.Logger.Log("Archive: Saving File: " + Path);

                //var fileName = Path.Combine(archiveBasePath, PersistenceUtilities.CookedFilename(file.Name, fileExtension, true));
                using (var outfile = new BinaryWriter(File.Open(archivePath, FileMode.Create)))
                {
                    outfile.Write(content);
                }
                /*
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
                */

            }
            catch (Exception e)
            {
                SafeHouse.Logger.Log(e);
                return false;
            }

            return true;

        }

        public override int Size { get { return Read().Length; } }

        public override byte[] Read()
        {
            archive.CreateArchiveDirectory();

            using (var infile = new BinaryReader(File.Open(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                return ProcessBinaryReader(infile);
            }

        }

        public override bool Append(byte[] content)
        {
            archive.CreateArchiveDirectory();

            // Using binary writer so we can bypass the OS behavior about ASCII end-of-lines and always use \n's no matter the OS:
            // Deliberately not catching potential I/O exceptions from this, so they will percolate upward and be seen by the user:
            using (var outfile = new BinaryWriter(File.Open(archivePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                //byte[] binaryLine = System.Text.Encoding.UTF8.GetBytes((textToAppend + "\n").ToCharArray());
                outfile.Write(content);
            }
            return true;
        }


        private byte[] ProcessBinaryReader(BinaryReader infile)
        {
            const int BUFFER_SIZE = 4096;
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[BUFFER_SIZE];
                int count;
                while ((count = infile.Read(buffer, 0, buffer.Length)) != 0)
                    ms.Write(buffer, 0, count);
                return ms.ToArray();
            }
        }
    }
}

