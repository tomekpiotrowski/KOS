using System;
using NUnit.Framework;
using kOS.Safe.Persistence;

namespace kOS.Safe.Test
{
    [TestFixture]
    public class VolumePathTest
    {
        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandleEmptyPath()
        {
            VolumePath.FromString("");
        }

        [Test]
        public void CanHandleRootPath()
        {
            VolumePath path = VolumePath.FromString("/");
            Assert.AreEqual(0, path.Length);
            Assert.AreEqual(0, path.Depth);
        }

        [Test]
        public void CanHandleSimplePath()
        {
            VolumePath path = VolumePath.FromString("/identifier");
            Assert.AreEqual(1, path.Length);
            Assert.AreEqual(1, path.Depth);
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandleInvalidIdentifier()
        {
            VolumePath.FromString("/identifier*+");
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandleAbsolutePathWithParent()
        {
            VolumePath parent = VolumePath.FromString("/parent");
            VolumePath.FromString("/identifier", parent);
        }

        [Test]
        public void CanHandleTwoDots()
        {
            VolumePath parent = VolumePath.FromString("/parent/deeper/and_deeper");
            VolumePath path = VolumePath.FromString("../../", parent);
            Assert.AreEqual(1, path.Depth);
            Assert.AreEqual(1, path.Length);
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandlePathsThatPointOutside1()
        {
            VolumePath.FromString("/..");
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandlePathsThatPointOutside2()
        {
            VolumePath.FromString("/../test/test/test");
        }

        [Test]
        public void CanReturnParent()
        {
            GlobalPath path = GlobalPath.FromString("othervolume:/level1/level2");
            Assert.AreEqual("othervolume", path.VolumeId);
            Assert.AreEqual(2, path.Depth);
            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void CanIdentifyParents()
        {
            GlobalPath path = GlobalPath.FromString("othervolume:/level1/level2");
            GlobalPath parent1 = GlobalPath.FromString("othervolume:");
            GlobalPath parent2 = GlobalPath.FromString("othervolume:/level1");
            GlobalPath notParent1 = GlobalPath.FromString("othervolume:/sthelse");
            GlobalPath notParent2 = GlobalPath.FromString("othervolume:/level1/level2/level3");
            GlobalPath notParent3 = GlobalPath.FromString("othervolume2:/level1/level2");

            Assert.IsTrue(parent1.IsParent(path));
            Assert.IsTrue(parent2.IsParent(path));
            Assert.IsFalse(path.IsParent(path));
            Assert.IsFalse(notParent1.IsParent(path));
            Assert.IsFalse(notParent2.IsParent(path));
            Assert.IsFalse(notParent3.IsParent(path));
        }

        [Test]
        public void CanHandleVolumeNames()
        {
            GlobalPath path = GlobalPath.FromString("othervolume:/level1/level2");
            Assert.AreEqual("othervolume", path.VolumeId);
            Assert.AreEqual(2, path.Depth);
            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void CanHandleVolumeIds()
        {
            GlobalPath path = GlobalPath.FromString("1:level1/level2");
            Assert.AreEqual(1, path.VolumeId);
            Assert.AreEqual(2, path.Depth);
            Assert.AreEqual(2, path.Length);
        }

        [Test]
        public void CanHandleJustVolumeName()
        {
            GlobalPath path = GlobalPath.FromString("othervolume:");
            Assert.AreEqual("othervolume", path.VolumeId);
            Assert.AreEqual(0, path.Depth);
            Assert.AreEqual(0, path.Length);
        }

        [Test]
        [ExpectedException(typeof(KOSInvalidPathException))]
        public void CanHandleGlobalPathWithLessThanZeroDepth()
        {
            GlobalPath.FromString("othervolume:/../");
        }

    }
}

