using System;

namespace Common.Exceptions
{
    /// <summary>
    ///     Klasa do reprezentacji wyjątu rzucanego podczas niepowodzenia w wysłaniu
    ///     wiadomości każdego typu
    /// </summary>
    public class MessageNotSentException : Exception
    {
        public MessageNotSentException()
        {
        }

        public MessageNotSentException(string message) : base(message)
        {
        }

        public MessageNotSentException(string message, Exception e) : base(message, e)
        {
        }
    }
}