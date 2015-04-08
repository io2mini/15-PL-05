using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    /// <summary>
    /// Klasa reprezentująca własny wyjątek rzucany, gdny nie możemy nawiązac połączenia z 
    /// Componentem
    /// </summary>
    public class ConnectionException : Exception
    {
        public ConnectionException() : base() { }

        public ConnectionException(String message) : base(message) { }

        public ConnectionException(String message, Exception e) : base(message, e) { }
    }
}
