using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Configuration
{
    public class ThreadInfo
    {
        public ComputationalThread[] Threads;
        public ThreadInfo(int n)
        {
            var Threads = new List<ComputationalThread>();
            for (int i = 0; i < n; i++) Threads.Add(new ComputationalThread());
            this.Threads = Threads.ToArray();
        }
    }
}
