using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class SolutionNotFoundException : Exception
    {
        public SolutionNotFoundException() : base() { }

        public SolutionNotFoundException(String message) : base(message) { }

        public SolutionNotFoundException(String message, Exception e) : base(message, e) { }
    }
}
