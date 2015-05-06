using System;

namespace Common.Exceptions
{
    /// <summary>
    ///     Klasa do reprezentacji wyjątku rzucanego podczas próbie wysłania status report
    ///     komponenta o id mniejszym od 0
    /// </summary>
    public class InvalidIdException : Exception
    {
        public InvalidIdException()
        {
        }

        public InvalidIdException(string message) : base(message)
        {
        }

        public InvalidIdException(string message, Exception e) : base(message, e)
        {
        }
    }
}