using System;
using kOS.Safe.Serialization;
using kOS.Safe.Encapsulation;
using System.Collections.Generic;
using kOS.Safe.Exceptions;
using kOS.Safe.Utilities;
using kOS.Safe;

namespace kOS.Serialization
{
    public class SerializationMgr : SafeSerializationMgr
    {
        static SerializationMgr() {
            SafeSerializationMgr.AddAssembly(typeof(SerializationMgr).Assembly.FullName);
        }

        private SharedObjects sharedObjects;

        public SerializationMgr(SharedObjects sharedObjects) : base()
        {
            this.sharedObjects = sharedObjects;
        }

        public override IDumper CreateAndLoad(string typeFullName, Dump data)
        {
            IDumper instance = base.CreateInstance(typeFullName);

            if (instance is IDumperWithSharedObjects)
            {
                IDumperWithSharedObjects withSharedObjects = instance as IDumperWithSharedObjects;
                withSharedObjects.SetSharedObjects(sharedObjects);
            }

            instance.LoadDump(data);

            return instance;
        }
    }
}

