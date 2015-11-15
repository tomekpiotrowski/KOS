using System;
using kOS.Safe.Function;
using kOS.Safe.Persistence;
using kOS.Safe.Utilities;
using KSP.IO;
using kOS.Safe;
using kOS.Safe.Exceptions;

namespace kOS.Function
{
    [Function("switch")]
    public class FunctionSwitch : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            object volumeId = PopValueAssert(shared, true);
            AssertArgBottomAndConsume(shared);

            if (shared.VolumeMgr != null)
            {
                Volume volume = volumeId is Volume ? volumeId as Volume : shared.VolumeMgr.GetVolume(volumeId);
                if (volume != null)
                {
                    shared.VolumeMgr.SwitchTo(volume);
                }
                else
                {
                    throw new Exception("Volume not found");
                }
            }
        }
    }
    
    [Function("edit")]
    public class FunctionEdit : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string pathString = PopValueAssert(shared, true).ToString();
            AssertArgBottomAndConsume(shared);

            // If no filename extension, then give it one:
            //pathString = PersistenceUtilities.CookedFilename(pathString, Volume.KERBOSCRIPT_EXTENSION);

            GlobalPath path = shared.VolumeMgr.GlobalPathFromString(pathString);

            if (shared.VolumeMgr != null)
            {
                Volume vol = shared.VolumeMgr.FromPath(path);
                shared.Window.OpenPopupEditor(vol, path);
            }
        }
    }

    [Function("cd")]
    public class FunctionCd : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string pathString = PopValueAssert(shared, true).ToString();
            AssertArgBottomAndConsume(shared);

            if (shared.VolumeMgr != null)
            {
                GlobalPath path = shared.VolumeMgr.GlobalPathFromString(pathString);
                Volume volume = shared.VolumeMgr.FromPath(path);

                VolumeDirectory directory = volume.Get(path) as VolumeDirectory;

                if (directory == null)
                {
                    throw new KOSException("Invalid directory: " + pathString);
                }

                shared.VolumeMgr.CurrentDirectory = directory;
            }
        }
    }

    [Function("copy")]
    public class FunctionCopy : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string destinationPathString = PopValueAssert(shared, true).ToString();
            string sourcePathString = PopValueAssert(shared, true).ToString();
            AssertArgBottomAndConsume(shared);

            SafeHouse.Logger.Log(string.Format("FunctionCopy: {0} {1}", sourcePathString, destinationPathString));

            if (shared.VolumeMgr != null)
            {
                GlobalPath sourcePath = shared.VolumeMgr.GlobalPathFromString(sourcePathString);
                GlobalPath destinationPath = shared.VolumeMgr.GlobalPathFromString(destinationPathString);

                Volume sourceVolume = shared.VolumeMgr.FromPath(sourcePath);
                Volume destinationVolume = shared.VolumeMgr.FromPath(destinationPath);

                VolumeItem source = sourceVolume.Get(sourcePath);
                VolumeItem destination = destinationVolume.Get(sourcePath);
            
                if (source is VolumeDirectory)
                {
                    if (destination is VolumeFile)
                    {
                        throw new KOSException("Can't copy directory into a file");
                    }

                    VolumeDirectory destinationDirectory = destination as VolumeDirectory;


                    throw new NotImplementedException();
                }
                else
                {
                    if (destination is VolumeDirectory)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

            }
        }
    }

    [Function("move")]
    public class FunctionMove : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string destinationPathString = PopValueAssert(shared, true).ToString();
            string sourcePathString = PopValueAssert(shared, true).ToString();
            AssertArgBottomAndConsume(shared);

            if (shared.VolumeMgr != null)
            {
                GlobalPath sourcePath = shared.VolumeMgr.GlobalPathFromString(sourcePathString);
                GlobalPath destinationPath = shared.VolumeMgr.GlobalPathFromString(destinationPathString);

                Volume sourceVolume = shared.VolumeMgr.FromPath(sourcePath);
                Volume destinationVolume = shared.VolumeMgr.FromPath(destinationPath);

                VolumeItem source = sourceVolume.Get(sourcePath);
                VolumeItem destination = destinationVolume.Get(sourcePath);

                if (source is VolumeDirectory)
                {
                    if (destination is VolumeFile)
                    {
                        throw new KOSException("Can't copy directory into a file");
                    }

                    VolumeDirectory destinationDirectory = destination as VolumeDirectory;


                    throw new NotImplementedException();
                }
                else
                {
                    if (destination is VolumeDirectory)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

        }
    }

    [Function("delete")]
    public class FunctionDelete : FunctionBase
    {
        public override void Execute(SharedObjects shared)
        {
            string pathString = PopValueAssert(shared, true).ToString();
            AssertArgBottomAndConsume(shared);

            if (shared.VolumeMgr != null)
            {
                GlobalPath path = shared.VolumeMgr.GlobalPathFromString(pathString);
                Volume volume = shared.VolumeMgr.FromPath(path);
                VolumeItem item = volume.Get(path);

                item.Delete();
            }
        }
    }


}
