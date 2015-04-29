using System;

namespace Common.Exceptions
{
    public class TaskManagerNotIdleException : Exception
    {
        public TaskManagerNotIdleException()
        {
        }

        public TaskManagerNotIdleException(string message) : base(message)
        {
        }

        public TaskManagerNotIdleException(string message, Exception e) : base(message, e)
        {
        }
    }
}