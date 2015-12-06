using System.Collections.Generic;
using kOS.Safe.Serialization;

namespace kOS.Safe.Encapsulation
{
    /// <summary>
    /// Classes implementing this interface can dump their data to a dictionary.
    /// </summary>
    public interface IDumper
    {
        Dump Dump();
        void LoadDump(Dump dump);
    }
}