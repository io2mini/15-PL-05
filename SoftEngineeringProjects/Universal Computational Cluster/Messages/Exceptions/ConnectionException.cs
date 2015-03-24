using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException() : base() { }

        public ConnectionException(String message) : base(message) { }

        public ConnectionException(String message, Exception e) : base(message, e) { }
    }
}
