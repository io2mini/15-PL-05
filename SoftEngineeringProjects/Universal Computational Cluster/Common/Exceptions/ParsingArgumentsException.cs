using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class ParsingArgumentsException : Exception
    {
        public ParsingArgumentsException() : base() { }

        public ParsingArgumentsException(String message) : base(message) { }

        public ParsingArgumentsException(String message, Exception e) : base(message, e) { }
    }
}
