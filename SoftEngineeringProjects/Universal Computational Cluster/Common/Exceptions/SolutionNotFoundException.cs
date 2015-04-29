using System;

namespace Common.Exceptions
{
    public class SolutionNotFoundException : Exception
    {
        public SolutionNotFoundException()
        {
        }

        public SolutionNotFoundException(string message) : base(message)
        {
        }

        public SolutionNotFoundException(string message, Exception e) : base(message, e)
        {
        }
    }
}