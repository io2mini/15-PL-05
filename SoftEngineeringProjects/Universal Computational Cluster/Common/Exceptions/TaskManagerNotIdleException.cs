using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class TaskManagerNotIdleException : Exception
    {
        public TaskManagerNotIdleException() : base() { }

        public TaskManagerNotIdleException(String message) : base(message) { }

        public TaskManagerNotIdleException(String message, Exception e) : base(message, e) { }
    }
}
