using System;
using System.Collections.Generic;
using System.Linq;


namespace kOS.Safe.Serialization
{
    public class JSONFormatter : Formatter
    {
        private static readonly JSONFormatter instance = new JSONFormatter();

        public static JSONFormatter Instance
        {
            get 
            {
                return instance; 
            }
        }

        private JSONFormatter()
        {
        }

        private object MakeStringDictionaries(object value)
        {
            if (value is IDictionary<object, object>)
            {
                Dictionary<string, object> stringKeys = new Dictionary<string, object>();

                foreach (KeyValuePair<object, object> entry in value as IDictionary<object, object>)
                {
                    stringKeys[entry.Key.ToString()] = MakeStringDictionaries(entry.Value);
                }

                return stringKeys;
            } else
            {
                return value;
            }
        }

        public string Write(Dump value)
        {
            return SimpleJson.SerializeObject(MakeStringDictionaries(value));
        }
            
        private object Unwrap(object read)
        {
            if (read is IDictionary<string, object>)
            {
                Dump result = new Dump();

                foreach (KeyValuePair<string, object> entry in read as IDictionary<string, object>)
                {
                    result[entry.Key] = Unwrap(entry.Value);
                }

                return result;
            } else
            {
                return read;
            }
        }

        public Dump Read(string input)
        {
            return (Dump)Unwrap(SimpleJson.DeserializeObject<IDictionary<string, object>>(input));
        }

    }
}

