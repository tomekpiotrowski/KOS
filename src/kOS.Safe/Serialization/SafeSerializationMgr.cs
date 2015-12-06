using System;
using System.Collections;
using System.Collections.Generic;
using kOS.Safe.Exceptions;
using kOS.Safe.Encapsulation;
using System.Reflection;

namespace kOS.Safe.Serialization
{
    public class SafeSerializationMgr
    {
        public static string TYPE_KEY = "$type";
        private static HashSet<string> assemblies = new HashSet<string>();

        public static void AddAssembly(string assembly)
        {
            assemblies.Add(assembly);
        }

        public SafeSerializationMgr()
        {

        }

        public static bool IsValue(object serialized)
        {
            return serialized.GetType().IsPrimitive || serialized is string;
        }

        public Dump Dump(IDumper dumper, bool includeType = true)
        {
            var dumped = dumper.Dump();

            List<object> keys = new List<object>(dumped.Keys);

            foreach (object key in keys)
            {
                if (dumped[key] is IDumper) {
                    dumped[key] = Dump(dumped [key] as IDumper, includeType);
                } else if (IsValue(dumped[key]) | dumped[key] is Dump)
                {
                    // Dumps and values are ok
                } else
                {
                    throw new KOSException("This type can't be serialized: " + dumped[key].GetType().Name);
                }
            }

            if (includeType)
            {
                dumped.Add(TYPE_KEY, dumper.GetType().Namespace + "." + dumper.GetType().Name);
            }

            return dumped;
        }

        public string Serialize(IDumper serialized, Formatter formatter, bool includeType = true)
        {
            return formatter.Write(Dump(serialized, includeType));
        }

        public IDumper CreateFromDump(Dump dump)
        {
            Dump data = new Dump();
            foreach (KeyValuePair<object, object> entry in dump)
            {
                if (entry.Key.Equals(TYPE_KEY))
                {
                    continue;
                }

                if (entry.Value is Dump)
                {
                    data[entry.Key] = CreateFromDump(entry.Value as Dump);
                } else
                {
                    data[entry.Key] = entry.Value;
                }
            }

            if (!dump.ContainsKey(TYPE_KEY))
            {
                throw new KOSSerializationException("Type information missing");
            }

            string typeFullName = dump[TYPE_KEY] as string;

            return CreateAndLoad(typeFullName, data);
        }

        public virtual IDumper CreateAndLoad(string typeFullName, Dump data)
        {
            IDumper instance = CreateInstance(typeFullName);

            instance.LoadDump(data);

            return instance;
        }

        public virtual IDumper CreateInstance(string typeFullName)
        {
            var deserializedType = Type.GetType(typeFullName);


            if (deserializedType == null)
            {
                foreach (string assembly in assemblies)
                {
                    deserializedType = Type.GetType(typeFullName + ", " + assembly);
                    if (deserializedType != null)
                    {
                        break;
                    }
                }

            }

            if (deserializedType == null)
            {
                throw new KOSSerializationException("Unrecognized type: " + typeFullName);
            }

            return Activator.CreateInstance(deserializedType) as IDumper;
        }

        public object Deserialize(string input, Formatter formatter)
        {
            object serialized = formatter.Read(input);

            if (serialized is Dump)
            {
                return CreateFromDump(serialized as Dump);
            }

            return serialized;
        }

        public string ToString(IDumper dumper)
        {
            return Serialize(dumper, TerminalFormatter.Instance, false);
        }
    }
}

