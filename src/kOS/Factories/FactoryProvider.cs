using kOS.Factories;
using kOS.AddOns.RemoteTech;

namespace kOS
{
    public static class FactoryProvider
    {
        public static IFactory Get()
        {
            bool isAvailable;
            try
            {
                isAvailable = RemoteTechHook.IsAvailable();
            }
            catch
            {
                isAvailable = false;
            }

            return isAvailable ? new RemoteTechFactory() : new StandardFactory();
        }
    }
}
