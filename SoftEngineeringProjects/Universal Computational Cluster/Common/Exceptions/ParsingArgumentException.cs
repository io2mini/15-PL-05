using System;

namespace Common.Exceptions
{
    /// <summary>
    ///     Klasa do reprezentacji wyjątku rzucanego podczas złego sparsowania argumentów
    ///     wejściowych dla każdego komponentu
    /// </summary>
    public class ParsingArgumentException : Exception
    {
        public ParsingArgumentException()
        {
        }

        public ParsingArgumentException(string message) : base(message)
        {
        }

        public ParsingArgumentException(string message, Exception e) : base(message, e)
        {
        }
    }
}