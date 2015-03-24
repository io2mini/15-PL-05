using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class MessageNotSentException : Exception
    {
        public MessageNotSentException() : base() { }

        public MessageNotSentException(String message) : base(message) { }

        public MessageNotSentException(String message, Exception e) : base(message, e) { }
    }
}
