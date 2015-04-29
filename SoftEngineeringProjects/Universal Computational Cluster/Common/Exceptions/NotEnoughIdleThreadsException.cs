using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class NotEnoughIdleThreadsException : Exception
    {
        public NotEnoughIdleThreadsException() : base() { }

        public NotEnoughIdleThreadsException(String message) : base(message) { }

        public NotEnoughIdleThreadsException(String message, Exception e) : base(message, e) { }
    }
    
}
