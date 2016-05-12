using kOS.Execution;
using kOS.Communication;
using kOS.Binding;
using kOS.Factories;
using kOS.Screen;

namespace kOS
{
    public class SharedObjects : Safe.SafeSharedObjects
    {
        public ConnectivityManager ConnectivityMgr { get; set; }
        //public IFactory Factory { get; set; }
        public TermWindow Window { get; set; }
    }
}