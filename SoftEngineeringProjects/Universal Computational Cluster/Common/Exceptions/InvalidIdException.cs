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
    public class InvalidIdException : Exception
    {
        public InvalidIdException() : base() { }

        public InvalidIdException(String message) : base(message) { }

        public InvalidIdException(String message, Exception e) : base(message, e) { }
    }

}
