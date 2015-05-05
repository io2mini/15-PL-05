using System;
using System.Collections.Generic;

namespace Common.Configuration
{
    public class ThreadInfo
    {
        public List<ComputationalThread> Threads;
        public SystemComponent Parent;
        public ThreadInfo(int n,SystemComponent Parent)
        {
            this.Parent = Parent;
            var threads = new List<ComputationalThread>();
            for (var i = 0; i < n; i++) threads.Add(new ComputationalThread());
            Threads = threads;
        }

        internal void SolutionCallback(byte[] p,ComputationalThread T)
        {
            throw new NotImplementedException();
        }
    }
}