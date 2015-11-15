using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using kOS.AddOns.RemoteTech;
using kOS.Safe.Persistence;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using kOS.Safe;

namespace kOS.Persistence
{
    public static class PersistenceExtensions
    {
        private const string FILENAME_VALUE_STRING = "filename";
        private const string DIRNAME_VALUE_STRING = "dirname";

        public static Harddisk ToHardDisk(this ConfigNode configNode)
        {
            var capacity = 10000;
            if (configNode.HasValue("capacity")) capacity = int.Parse(configNode.GetValue("capacity"));

            var toReturn = new Harddisk(capacity);
            
            if (configNode.HasValue("volumeName")) toReturn.Name = configNode.GetValue("volumeName");

            toReturn.RootHarddiskDirectory = configNode.ToHarddiskDirectory(toReturn, VolumePath.EMPTY);

            return toReturn;
        }

        private static HarddiskDirectory ToHarddiskDirectory(this ConfigNode configNode, Harddisk harddisk, VolumePath parentPath)
        {
            string dirName = configNode.GetValue(DIRNAME_VALUE_STRING);
            HarddiskDirectory directory = new HarddiskDirectory(harddisk, VolumePath.FromString(dirName, parentPath));

            foreach (ConfigNode fileNode in configNode.GetNodes("file"))
            {
                directory.CreateFile(fileNode.GetValue(FILENAME_VALUE_STRING), fileNode.ToHarddiskFile(harddisk, directory.Path));
            }

            foreach (ConfigNode dirNode in configNode.GetNodes("directory"))
            {
                directory.CreateDirectory(dirName, dirNode.ToHarddiskDirectory(harddisk, VolumePath.FromString(dirName, parentPath)));
            }

            return directory;
        }

        public static HarddiskFile ToHarddiskFile(this ConfigNode configNode, Harddisk harddisk, VolumePath parentPath)
        {
            var filename = configNode.GetValue(FILENAME_VALUE_STRING);
            var toReturn = new HarddiskFile(harddisk, VolumePath.FromString(filename, parentPath));

            toReturn.Content = Decode(configNode.GetValue("line"));
            return toReturn;
        }

        public static ConfigNode ToConfigNode(this Harddisk harddisk, string nodeName)
        {
            var node = harddisk.RootHarddiskDirectory.ToConfigNode(nodeName);

            node.AddValue("capacity", harddisk.Capacity);
            node.AddValue("volumeName", harddisk.Name);

            return node;
        }

        public static ConfigNode ToConfigNode(this HarddiskDirectory directory, string nodeName)
        {
            ConfigNode node = new ConfigNode(nodeName);
            node.AddValue(DIRNAME_VALUE_STRING, directory.Name);

            foreach (VolumeItem item in directory)
            {
                if (item is HarddiskDirectory)
                {
                    HarddiskDirectory dir = item as HarddiskDirectory;
                    node.AddNode(dir.ToConfigNode("directory"));
                }

                if (item is HarddiskFile)
                {
                    HarddiskFile file = item as HarddiskFile;
                    node.AddNode(file.ToConfigNode("file"));
                }
            }

            return node;
        }

        public static ConfigNode ToConfigNode(this HarddiskFile harddiskFile, string nodeName)
        {
            var node = new ConfigNode(nodeName);
            node.AddValue(FILENAME_VALUE_STRING, harddiskFile.Name);

            node.AddValue("line", Encode(harddiskFile.Content, harddiskFile.Category));

            return node;
        }

        private static string Encode(byte[] input, FileCategory category)
        {
            if (category == FileCategory.KSM)
            {
                return EncodeBase64(input);
            }
            else
            {
                if (Config.Instance.UseCompressedPersistence)
                {
                    return EncodeBase64(input);
                }
                else
                {
                    return PersistenceUtilities.EncodeLine(input);
                }
            }
        }

        private static byte[] Decode(string input)
        {
            try
            {
                // base64 encoding

                // Fix for issue #429.  See comment up in EncodeBase64() method above for an explanation:
                string massagedInput = input.Replace(',','/');

                return DecodeBase64ToBinary(massagedInput);
            }
            catch (FormatException)
            {
                // standard encoding
                return PersistenceUtilities.DecodeLine(input);
            }
            /*
            catch (Exception e)
            {
                SafeHouse.Logger.Log(string.Format("Exception decoding: {0} | {1}", e, e.Message));
            }*/
        }

        private static string EncodeBase64(byte[] input)
        {
            using (var compressedStream = new MemoryStream())
            {
                // mono requires an installed zlib library for GZipStream to work :(
                // using (Stream csStream = new GZipStream(compressedStream, CompressionMode.Compress))
                using (Stream csStream = new GZipOutputStream(compressedStream))
                {
                    csStream.Write(input, 0, input.Length);
                }

                string returnValue = Convert.ToBase64String(compressedStream.ToArray());
                
                // Added the following to fix issue #429:  Base64 content can include the slash character '/', and
                // if it happens to have two of them contiguously, it forms a comment in the persistence file and
                // truncates the value.  So change them to a different character to protect the file.
                // The comma ',' char is not used by base64 so it's a safe alternative to use as we'll be able to
                // swap all of the commas back to slashes on reading, knowing that commas can only appear as the
                // result of this swap on writing:
                returnValue = returnValue.Replace('/',',');

                SafeHouse.Logger.SuperVerbose("About to store the following Base64 string:\n" + returnValue);

                return returnValue;
            }
        }



        public static bool CheckRange(this Volume volume, Vessel vessel)
        {
            var archive = volume as RemoteTechArchive;
            return archive == null || archive.CheckRange(vessel);
        }

        // Provide a way to check the range limit of the archive without requesting the current volume (which throws an error if not in range)
        public static bool CheckCurrentVolumeRange(this VolumeManager volumeManager, Vessel vessel)
        {
            var rtManager = volumeManager as RemoteTechVolumeManager;
            if (rtManager == null) return true;
            return rtManager.CheckCurrentVolumeRange(vessel);
        }

        private static byte[] DecodeBase64ToBinary(string input)
        {
            byte[] inputBuffer = Convert.FromBase64String(input);

            using (var inputStream = new MemoryStream(inputBuffer))
            // mono requires an installed zlib library for GZipStream to work :(
            //using (var zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var zipStream = new GZipInputStream(inputStream))
            using (var decompressedStream = new MemoryStream())
            {
                var buffer = new byte[4096];
                int read;

                while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    decompressedStream.Write(buffer, 0, read);
                }

                return decompressedStream.ToArray();
            }
        }

    }
}
