using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public class TaskSolver : UCCTaskSolver.TaskSolver
    {
        private Problem _problem;

        public TaskSolver(byte[] problemData) : base(problemData)
        {
             _problem = Problem.Deserialize(problemData);

        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            
            throw new NotImplementedException();
        }

        public override byte[] MergeSolution(byte[][] solutions)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
