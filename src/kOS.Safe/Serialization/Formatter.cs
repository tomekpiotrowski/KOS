using System;
using System.Collections.Generic;

namespace kOS.Safe.Serialization
{
    public interface Formatter
    {
        string Write(Dump value);
        Dump Read(string input);
    }
}

