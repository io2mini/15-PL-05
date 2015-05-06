using System.Collections.Generic;
using Common.Messages;

namespace Common.Configuration
{
    public class ThreadInfo
    {
        public List<ComputationalThread> Threads;
        public SystemComponent Parent;
        public ThreadInfo(int n,SystemComponent parent)
        {
            Parent = parent;
            var threads = new List<ComputationalThread>();
            for (var i = 0; i < n; i++) threads.Add(new ComputationalThread());
            Threads = threads;
        }

        
        public void SetStateAll(StatusThreadState status)
        {
            foreach (var thread in Threads)
            {
                thread.SetStatus(status);
            }
        }
    }
}