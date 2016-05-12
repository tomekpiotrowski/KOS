using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Compilation;
using kOS.Safe.Persistence;
using kOS.Safe.Utilities;

namespace kOS.Safe.Processor
{
    public class ProcessorManager
    {
        // Use the attached volume as processor identifier
        public Dictionary<Volume, IProcessor> processors { get; private set; }

        public ProcessorManager()
        {
            processors = new Dictionary<Volume, IProcessor>();
        }

        public void UpdateProcessors(List<IProcessor> processorList)
        {
            processors.Clear();
            foreach (IProcessor processor in processorList)
            {
                processors.Add(processor.Volume, processor);
            }
        }

        public IProcessor GetProcessor(string name)
        {
            foreach (KeyValuePair<Volume, IProcessor> pair in processors)
            {
                if (pair.Value.Tag != null && String.Equals(pair.Value.Tag, name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public IProcessor GetProcessor(Volume volume)
        {
            if (processors.ContainsKey(volume))
            {
                return processors[volume];
            }
            throw new Exception("The volume is not attached to any processor");
        }
    }
}
