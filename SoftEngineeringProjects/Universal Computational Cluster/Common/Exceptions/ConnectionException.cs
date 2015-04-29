using System;

namespace Common.Exceptions
{
    /// <summary>
    ///     Klasa reprezentująca własny wyjątek rzucany, gdny nie możemy nawiązac połączenia z
    ///     Componentem
    /// </summary>
    public class ConnectionException : Exception
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        public ConnectionException(string message, Exception e) : base(message, e)
        {
        }
    }
}