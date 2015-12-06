using kOS.Safe.Encapsulation.Suffixes;
using kOS.Module;
using kOS.Safe.Persistence;
using kOS.Safe.Encapsulation;
using System;

namespace kOS.Suffixed.PartModuleField
{
    public class kOSProcessorFields : PartModuleFields
    {
        protected readonly kOSProcessor processor;

        public kOSProcessorFields(kOSProcessor processor, SharedObjects sharedObj):base(processor, sharedObj)
        {
            this.processor = processor;
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("MODE", new NoArgsSuffix<string>(() => processor.ProcessorMode.ToString(), "This processor's mode"));
            AddSuffix("ACTIVATE", new NoArgsSuffix(() => processor.ProcessorMode = kOS.Safe.Module.ProcessorModes.STARVED, "Activate this processor"));
            AddSuffix("DEACTIVATE", new NoArgsSuffix(() => processor.ProcessorMode = kOS.Safe.Module.ProcessorModes.OFF, "Deactivate this processor"));
            AddSuffix("VOLUME", new NoArgsSuffix<Volume>(() => processor.HardDisk, "This processor's hard disk"));
            AddSuffix("TAG", new NoArgsSuffix<string>(() => processor.Tag, "This processor's tag"));
            AddSuffix("BOOTFILENAME", new SetSuffix<string>(GetBootFilename, SetBootFilename, "The name of the processor's boot file."));
            AddSuffix("SENDMESSAGE", new OneArgsSuffix<object>((content) => processor.Send(content), "Send a message to this processor"));
        }

        private string GetBootFilename()
        {
            return processor.BootFilename;
        }

        private void SetBootFilename(string name)
        {
            processor.BootFilename = name;
        }
    }
}
