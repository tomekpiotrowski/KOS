using kOS.Safe.Persistence;
using kOS.Safe.Processor;
using kOS.Communication;
using kOS.Safe.Encapsulation;
using kOS.Safe.Screen;
using kOS.Binding;
using kOS.Function;
using kOS.Factories;
using UnityEngine;

namespace kOS.Processor
{
    /// <summary>
    /// Base class for any processors that are to be available in-game.
    /// </summary>
    public abstract class Processor<V, S> : BaseProcessor<MessageQueue, Message, V, S> where V : Volume
        where S : SharedObjects
    {
        protected Processor() : base(new MessageQueue())
        {

        }

        public void Initialize()
        {
            IFactory factory = FactoryProvider.Get();

            base.Initialize(factory.CreateVolumeManager());

            shared.BindingMgr = new BindingManager(shared);
            shared.Logger = new KSPLogger(shared);
            shared.FunctionManager = new FunctionManager(shared);
            shared.Interpreter = factory.CreateInterpreter(shared);
            shared.Screen = shared.Interpreter;
            shared.ConnectivityMgr = factory.CreateConnectivityManager();
            shared.SoundMaker = Sound.SoundMaker.Instance;


            // Make the window that is going to correspond to this kOS part:
            var gObj = new GameObject("kOSTermWindow", typeof(Screen.TermWindow));
            DontDestroyOnLoad(gObj);
            shared.Window = (Screen.TermWindow)gObj.GetComponent(typeof(Screen.TermWindow));
            shared.Window.AttachTo(shared);
        }

        public void OpenWindow()
        {
            shared.Window.Open();
        }

        public void CloseWindow()
        {
            shared.Window.Close();
        }

        public void ToggleWindow()
        {
            shared.Window.Toggle();
        }

        public bool WindowIsOpen()
        {
            return shared.Window.IsOpen;
        }

        public bool TelnetIsAttached()
        {
            return shared.Window.NumTelnets() > 0;
        }

        public IScreenBuffer GetScreen()
        {
            return shared.Screen;
        }

        // TODO - later refactor making this kOS.Safer so it can work on ITermWindow, which also means moving all of UserIO's classes too.
        public Screen.TermWindow GetWindow()
        {
            return shared.Window;
        }

        public void Send(Structure content)
        {
            double sentAt = Planetarium.GetUniversalTime();
            Messages.Push(Message.Create(content, sentAt, sentAt, new VesselTarget(shared), Tag));
        }
    }
}

