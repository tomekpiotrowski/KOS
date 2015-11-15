using System;
using kOS.Safe.Persistence;

namespace kOS.Safe.Persistence
{
    public class CookedVolume : Volume
    {
        private Volume volume;
        public CookedVolume(Volume volume)
        {
            this.volume = volume;
        }

        public override VolumeItem Get(VolumePath path)
        {
            throw new NotImplementedException();
        }

        #region implemented abstract members of Volume

        public override VolumeDirectory Root {
            get {
                return volume.Root;
            }
        }

        #endregion

        #region implemented abstract members of Volume
        public override VolumeDirectory CreateDirectory(VolumePath path)
        {
            throw new NotImplementedException();
        }
        public override VolumeFile CreateFile(VolumePath path)
        {
            throw new NotImplementedException();
        }
        #endregion            

        public override int Capacity {
            get {
                throw new NotImplementedException();
            }
        }

        public override bool Renameable {
            get {
                throw new NotImplementedException();
            }
        }

        public override float RequiredPower {
            get {
                throw new NotImplementedException();
            }
        }
             
        /// <summary>
        /// This is both for error checking and blessing of user-created filenames,
        /// and to tack on a filename extension if there is none present.
        /// <br/><br/><br/>
        /// Returns a version of the filename in which it has had the file extension
        /// added unless the filename already has any sort of file extension, in
        /// which case nothing is changed about it.  If every place where the auto-
        /// extension-appending is attempted is done via this method, then it will never end
        /// up adding an extension when an explicit one exists already.
        /// </summary>
        /// <param name="fileName">Filename to maybe change.  Can be full path or just the filename.</param>
        /// <param name="extensionName">Extension to add if there is none already.</param>
        /// <param name="trusted">True if the filename is internally generated (and therefore allowed to
        ///   have paths in it).  False if the filename is from a user-land string (and therefore allowing
        ///   a name that walks the directory tree is a security hole.)</param>
        /// <returns></returns>
        /*
        private string CookedPath(VolumePath path, string extensionName, bool trusted = false)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new KOSFileException("Attempted to use an empty filename.");

            int lastDotIndex = fileName.LastIndexOf('.');
            int lastSlashIndex = fileName.LastIndexOfAny(new[] { '/', '\\' }); // both kinds of OS folder separator.

            if (!trusted)
            {
                // Later if we add user folder abilities, this may have to get more fancy about what is
                // and isn't allowed:
                if (fileName.Contains(".."))
                    throw new KOSFileException("kOS does not allow using consecutive dots ('..') in a filename.");
                if (lastSlashIndex >= 0)
                    throw new KOSFileException("kOS does not allow pathname characters ('/','\\') in a filename.");
            }

            if (lastSlashIndex == fileName.Length - 1)
                throw new KOSFileException("Attempted to use a filename consisting only of directory paths");

            if (lastDotIndex == lastSlashIndex + 1) // lastSlashIndex == -1 if no slashes so this also covers just passing in ".foo".
                throw new KOSFileException("Attempted to use a filename beginning with a period ('.') character.");

            if (lastDotIndex < 0 || lastDotIndex < lastSlashIndex) // If no dot in the tail part of the filename after any potential directory separators.
                return fileName + "." + extensionName;
            if (lastDotIndex == fileName.Length - 1) // There is a dot, but it's at the very last character, as in "myfile."
                return fileName + extensionName;

            return fileName;
        }
        */
    }
}

