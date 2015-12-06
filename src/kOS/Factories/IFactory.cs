﻿using kOS.Safe.Persistence;
using kOS.Safe.Screen;
using kOS.Persistence;
using kOS.Communication;

namespace kOS.Factories
{
    public interface IFactory
    {
        IInterpreter CreateInterpreter(SharedObjects shared);
        Archive CreateArchive();
        ConnectivityManager CreateConnectivityManager();
        VolumeManager CreateVolumeManager(SharedObjects sharedObjects);
    }
}
