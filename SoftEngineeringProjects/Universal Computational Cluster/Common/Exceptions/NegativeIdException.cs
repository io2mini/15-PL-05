using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    /// <summary>
    /// Klasa do reprezentacji wyjątku rzucanego podczas próbie wysłania status report
    /// komponenta o id < 0
    /// </summary>
    public class NegativeIdException : Exception
    {
        public NegativeIdException() : base() { }

        public NegativeIdException(String message) : base(message) { }

        public NegativeIdException(String message, Exception e) : base(message, e) { }
    }

}
