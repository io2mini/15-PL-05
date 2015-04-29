using System;

namespace Common.Exceptions
{
    public class NotEnoughIdleThreadsException : Exception
    {
        public NotEnoughIdleThreadsException()
        {
        }

        public NotEnoughIdleThreadsException(string message) : base(message)
        {
        }

        public NotEnoughIdleThreadsException(string message, Exception e) : base(message, e)
        {
        }
    }
}