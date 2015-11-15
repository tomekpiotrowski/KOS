using System;
using NUnit.Framework;
using System.IO;
using kOS.Safe.Persistence;
using kOS.Safe.Utilities;
using kOS.Safe.Exceptions;
using System.Text;

namespace kOS.Safe.Test
{
    public abstract class VolumeTest
    {
        public Volume TestVolume { get; set; }

        [SetUp]
        public void SetupLogger()
        {
            SafeHouse.Logger = new TestLogger();
        }

        [Test]
        public void CanCreateDirectories()
        {
            string dir1 = "/testdir", dir2 = "/abc";
            Assert.AreEqual(0, TestVolume.Root.List().Count);

            TestVolume.CreateDirectory(VolumePath.FromString(dir1));
            TestVolume.CreateDirectory(VolumePath.FromString(dir2));

            Assert.AreEqual(2, TestVolume.Root.List().Count);
            Assert.AreEqual(dir2, TestVolume.Root.List()[0].Path.ToString());
            Assert.AreEqual(dir1, TestVolume.Root.List()[1].Path.ToString());
        }

        [Test]
        public void CanCreateSubdirectories()
        {
            string parent1 = "/parent1", parent2 = "/parent2";
            string dir1 = parent1 + "/sub1", dir2 = parent1 + "/sub2", dir3 = parent2 + "/sub3";
            Assert.AreEqual(0, TestVolume.Root.List().Count);

            TestVolume.CreateDirectory(VolumePath.FromString(dir1));
            TestVolume.CreateDirectory(VolumePath.FromString(dir2));
            TestVolume.CreateDirectory(VolumePath.FromString(dir3));

            Assert.AreEqual(2, TestVolume.Root.List().Count);
            Assert.AreEqual(parent1, TestVolume.Root.List()[0].Path.ToString());
            Assert.AreEqual(parent2, TestVolume.Root.List()[1].Path.ToString());

            VolumeDirectory dir = TestVolume.Get(VolumePath.FromString(parent1)) as VolumeDirectory;
            Assert.AreEqual(2, dir.List().Count);
            Assert.AreEqual(dir1, dir.List()[0].Path.ToString());
            Assert.AreEqual(dir2, dir.List()[1].Path.ToString());
        }

        [Test]
        [ExpectedException(typeof(KOSPersistenceException))]
        public void CanFailWhenCreatingDirectoryOverFile()
        {
            string parent1 = "/parent1";
            string file1 = parent1 + "/sub1";

            TestVolume.CreateFile(VolumePath.FromString(file1));
            TestVolume.CreateDirectory(VolumePath.FromString(file1));
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanFailWhenCreatingDirectoryWithNegativeDepth()
        {
            string dir = "/../test";

            TestVolume.CreateDirectory(VolumePath.FromString(dir));
        }

        [Test]
        public void CanDeleteDirectories()
        {
            string parent1 = "/parent1", parent2 = "/parent2";
            string dir1 = parent1 + "/sub1", dir2 = parent1 + "/sub2", dir3 = parent2 + "/sub3";

            TestVolume.CreateDirectory(VolumePath.FromString(dir1));
            TestVolume.CreateDirectory(VolumePath.FromString(dir2));
            TestVolume.CreateDirectory(VolumePath.FromString(dir3));

            VolumeDirectory parent = TestVolume.Get(VolumePath.FromString(parent1)) as VolumeDirectory;

            VolumeDirectory Deleted1 = TestVolume.Get(VolumePath.FromString(dir1)) as VolumeDirectory;
            VolumeDirectory Deleted2 = TestVolume.Get(VolumePath.FromString(parent2)) as VolumeDirectory;

            Deleted1.Delete();
            Assert.AreEqual(1, parent.List().Count);
            Assert.AreEqual(dir2, parent.List()[0].Path.ToString());

            Deleted2.Delete();
            Assert.AreEqual(1, TestVolume.Root.List().Count);
            Assert.AreEqual(parent1, TestVolume.Root.List()[0].Path.ToString());
        }

        [Test]
        public void CanDeleteNonExistingDirectories()
        {
            VolumePath path = VolumePath.FromString("/abc");
            TestVolume.CreateDirectory(path);
            VolumeDirectory deleted = TestVolume.Get(path) as VolumeDirectory;

            // Delete the directory twice
            deleted.Delete();
            deleted.Delete();
        }

        [Test]
        public void CanCreateFiles()
        {
            string parent1 = "/parent1", parent2 = "/parent2";
            string file1 = parent1 + "/sub1", file2 = parent1 + "/sub2", file3 = parent2 + "/sub3";

            TestVolume.CreateFile(VolumePath.FromString(file1));
            TestVolume.CreateFile(VolumePath.FromString(file2));
            TestVolume.CreateFile(VolumePath.FromString(file3));

            Assert.AreEqual(2, TestVolume.Root.List().Count);
            Assert.AreEqual(parent1, TestVolume.Root.List()[0].Path.ToString());
            Assert.AreEqual(parent2, TestVolume.Root.List()[1].Path.ToString());

            VolumeDirectory dir = TestVolume.Get(VolumePath.FromString(parent1)) as VolumeDirectory;
            Assert.AreEqual(2, dir.List().Count);
            Assert.AreEqual(file1, dir.List()[0].Path.ToString());
            Assert.IsInstanceOf<VolumeFile>(dir.List()[0]);
            Assert.AreEqual(file2, dir.List()[1].Path.ToString());
            Assert.IsInstanceOf<VolumeFile>(dir.List()[1]);
        }


        [Test]
        [ExpectedException(typeof(KOSPersistenceException))]
        public void CanFailWhenCreatingFileOverDirectory()
        {
            string parent1 = "/parent1";
            string file1 = parent1 + "/sub1";

            TestVolume.CreateDirectory(VolumePath.FromString(file1));
            TestVolume.CreateFile(VolumePath.FromString(file1));
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanFailWhenCreatingFileWithNegativeDepth()
        {
            string dir = "/../test";

            TestVolume.CreateFile(VolumePath.FromString(dir));
        }

        [Test]
        public void CanReadAndWriteFiles()
        {
            string dir = "/content_parent/content_test";
            string content = "some test content!@#$;\n\rtaenstałąż";

            VolumeFile volumeFile = TestVolume.CreateFile(VolumePath.FromString(dir));

            Assert.AreEqual(new byte[0], volumeFile.Read());
            Assert.AreEqual("", volumeFile.ReadAsString());

            Assert.IsTrue(volumeFile.Write(content));
            Assert.AreEqual(FileCategory.ASCII, volumeFile.Category);

            Assert.AreEqual(Encoding.UTF8.GetBytes(content).Length, volumeFile.Size);
            Assert.AreEqual(content, volumeFile.ReadAsString());
        }

        [Test]
        public void CanDeleteFiles()
        {
            string parent1 = "/parent1", parent2 = "/parent2";
            string file1 = parent1 + "/sub1", file2 = parent1 + "/sub2", file3 = parent2 + "/sub3";

            VolumeFile removed1 = TestVolume.CreateFile(VolumePath.FromString(file1));
            TestVolume.CreateFile(VolumePath.FromString(file2));
            TestVolume.CreateFile(VolumePath.FromString(file3));

            removed1.Delete();

            VolumeDirectory dir = TestVolume.Get(VolumePath.FromString(parent1)) as VolumeDirectory;
            Assert.AreEqual(1, dir.List().Count);
            Assert.AreEqual(file2, dir.List()[0].Path.ToString());
            Assert.IsInstanceOf<VolumeFile>(dir.List()[0]);
        }

        [Test]
        public void CanDeleteNonExistingFiles()
        {
            VolumePath path = VolumePath.FromString("/abc");
            TestVolume.CreateFile(path);
            VolumeFile deleted = TestVolume.Get(path) as VolumeFile;

            // Delete the file twice
            deleted.Delete();
            deleted.Delete();
        }

        /*
        [Test]
        public void CanCopyFiles()
        {
            Assert.Fail();
        }

        [Test]
        public void CanMoveFiles()
        {
            Assert.Fail();
        }



        [Test]
        public void CanCopyDirectories()
        {
            Assert.Fail();
        }

        [Test]
        public void CanMoveDirectories()
        {
            Assert.Fail();
        }

        [Test]
        public void CanCalculateUsedAndFreeSpace()
        {
            Assert.Fail();
        }
        */            
    }
}

