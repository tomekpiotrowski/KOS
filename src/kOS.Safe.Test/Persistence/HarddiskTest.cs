using System;
using NUnit.Framework;
using kOS.Safe.Persistence;

namespace kOS.Safe.Test
{
    [TestFixture]
    public class HarddiskTest : VolumeTest
    {
        private int HarddiskSize = 5000;

        [SetUp]
        public void Setup()
        {
            TestVolume = new Harddisk(HarddiskSize);
        }

    }
}

