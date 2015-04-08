using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    /// <summary>
    /// Klasa do reprezentacji wyjątku rzucanego podczas złego sparsowania argumentów
    /// wejściowych dla każdego komponentu
    /// </summary>
    public class ParsingArgumentException : Exception
    {
        public ParsingArgumentException() : base() { }

        public ParsingArgumentException(String message) : base(message) { }

        public ParsingArgumentException(String message, Exception e) : base(message, e) { }
    }
}
