using System;
using kOS.Safe.Persistence;
using kOS.Safe.Communication;
using kOS.Safe.Compilation.KS;
using kOS.Safe.Execution;

namespace kOS.Safe.Processor
{
    /// <summary>
    /// Base class for all processors - those available in-game and those available outside.
    /// </summary>
    public abstract class BaseProcessor<MQ, M, V, S> : IProcessor where MQ : GenericMessageQueue<M>
        where M : BaseMessage where V : Volume where S : SafeSharedObjects
    {
        protected S shared;

        protected ProcessorModes ProcessorMode { get; private set; }

        public M Messages { get; private set; }

        protected BaseProcessor(M messages)
        {
            Messages = messages;
        }

        public void Initialize(IVolumeManager volumeMgr)
        {
            shared.Processor = this;
            shared.UpdateHandler = new UpdateHandler();
            shared.ScriptHandler = new KSScript();
            shared.Cpu = new CPU(shared);
            shared.ProcessorMgr = new ProcessorManager();
            shared.VolumeMgr = volumeMgr;
        }

        public void ToggleMode()
        {
            ProcessorModes newProcessorMode = (ProcessorMode != ProcessorModes.OFF) ? ProcessorModes.OFF : ProcessorModes.STARVED;
            SetMode(newProcessorMode);
        }

        public void SetMode(ProcessorModes newProcessorMode)
        {
            if (newProcessorMode != ProcessorMode)
            {
                ProcessorMode = newProcessorMode;

                ProcessorModeChanged();
            }
        }

        private void ProcessorModeChanged()
        {
            switch (ProcessorMode)
            {
            case ProcessorModes.READY:
                shared.VolumeMgr.SwitchTo(SafeHouse.Config.StartOnArchive
                    ? shared.VolumeMgr.GetVolume(0)
                    : HardDisk);
                if (shared.Cpu != null) shared.Cpu.Boot();
                if (shared.Interpreter != null) shared.Interpreter.SetInputLock(false);
                if (shared.Window != null) shared.Window.IsPowered = true;
                break;

            case ProcessorModes.OFF:
            case ProcessorModes.STARVED:
                if (shared.Interpreter != null) shared.Interpreter.SetInputLock(true);
                if (shared.Window != null) shared.Window.IsPowered = false;
                if (shared.BindingMgr != null) shared.BindingMgr.UnBindAll();
                break;
            }

        }

        private void UpdateObservers()
        {
            if (ProcessorMode == ProcessorModes.READY)
            {
                if (shared.UpdateHandler != null) shared.UpdateHandler.UpdateObservers(TimeWarp.deltaTime);
            }
        }

        private void UpdateFixedObservers()
        {
            if (ProcessorMode == ProcessorModes.READY)
            {
                if (shared.UpdateHandler != null) shared.UpdateHandler.UpdateFixedObservers(TimeWarp.fixedDeltaTime);
            }
        }
    }
}
