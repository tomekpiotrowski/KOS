using System;
using NUnit.Framework;
using System.IO;
using kOS.Safe.Persistence;
using kOS.Safe.Utilities;

namespace kOS.Safe.Test
{
    [TestFixture]
    public class ArchiveTest : VolumeTest
    {
        public const string KosTestDirectory = "kos_archive_tests";

        protected string testPath = Path.Combine(Path.GetTempPath(), KosTestDirectory);

        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(testPath))
            {
                Directory.Delete(testPath, true);
            }

            Directory.CreateDirectory(testPath);

            TestVolume = new Archive(testPath);
        }
    }
}

