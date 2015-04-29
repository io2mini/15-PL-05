using System.Collections.Generic;

namespace Common.Configuration
{
    public class ThreadInfo
    {
        public List<ComputationalThread> Threads;

        public ThreadInfo(int n)
        {
            var threads = new List<ComputationalThread>();
            for (var i = 0; i < n; i++) threads.Add(new ComputationalThread());
            Threads = threads;
        }
    }
}