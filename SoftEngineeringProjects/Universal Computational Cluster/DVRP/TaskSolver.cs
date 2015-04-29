using Common.Exceptions;
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
            var permuted_clients = Permuter.GeneratePermutations((uint)_problem.Clients.Count());
            // Extra depot means Depot where path cost = 0
            var combined_depots = Permuter.GenerateCombinations((uint)_problem.Clients.Count() + 1, (uint)(_problem.Depots.Count() + 1));
            throw new NotImplementedException();
        }

        public override byte[] MergeSolution(byte[][] byteSolutions)
        {
            if (byteSolutions == null) throw new ArgumentNullException();
            Solution solution = null;
            double cost = Double.MaxValue;
            foreach(byte[] byteArray in byteSolutions)
            {
                Solution solution_prim = Solution.Deserialize(byteArray);
                if(solution_prim.Cost < cost)
                {
                    cost = solution_prim.Cost;
                    solution = solution_prim;
                }
            }
            if (solution == null) throw new SolutionNotFoundException();
            return solution.Serialize();
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
