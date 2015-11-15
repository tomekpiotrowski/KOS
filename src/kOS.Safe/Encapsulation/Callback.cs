using System;
using System.Collections.Generic;
using kOS.Safe.Exceptions;
using kOS.Safe.Utilities;

namespace kOS.Safe.Encapsulation
{
    public abstract class Callback : ISuffixed, IOperable 
    {

        static Callback()
        {
        }


        public override string ToString()
        {
            return "Structure ";
        }

        #region IOperable implementation

        public object TryOperation (string op, object other, bool reverseOrder)
        {
            throw new NotImplementedException ();
        }

        #endregion

        #region ISuffixed implementation

        public bool SetSuffix (string suffixName, object value)
        {
            throw new NotImplementedException ();
        }

        public object GetSuffix (string suffixName)
        {
            throw new NotImplementedException ();
        }

        #endregion
    }
}
