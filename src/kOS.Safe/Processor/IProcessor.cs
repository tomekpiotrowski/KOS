using kOS.Safe.Persistence;

namespace kOS.Safe.Processor
{
    public interface IProcessor
    {
        Volume Volume { get; }

        void SetMode(ProcessorModes newProcessorMode);
        string BootFilename { get; set; }

        bool CheckCanBoot();
        string Name { get; }
    }
    public enum ProcessorModes { READY, STARVED, OFF };
}