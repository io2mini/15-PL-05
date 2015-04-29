using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
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
            //TODO: handle timespan
            var task = Task.Deserialize(partialData);
            double cost = double.MaxValue;
            uint[] bestSequence = null;
            foreach (var sequence in task.Sequences)
            {
                var costPrim = CountSigleVehicleCost(sequence, cost);
                if (costPrim < cost)
                {
                    cost = costPrim;
                    bestSequence = sequence;
                }
            }
            var solutionInput = new List<List<int>>();
            if (bestSequence != null)
            {
                var l = bestSequence.Cast<int>().ToList();
                solutionInput.Add(l);
            }
            var s = new Solution(solutionInput);
            return s.Serialize(); //Check for null?
        }

        public double CountSigleVehicleCost(uint[] idSequence, double bestCost)
        {
            bool s1 = _problem.Depots.Aggregate(false, (current, depot) => current || depot.Id == idSequence[0]);
            bool s2 = _problem.Depots.Aggregate(false, (current, depot) => current || depot.Id == idSequence[idSequence.Length - 1]);
            if (!s1 || !s2) return double.MaxValue; //Żaden z końców sekwencji nie jest depotem
            double cost = 0;
            double load = _problem.Fleet[0].Capacity;
            var actualTime = _problem.GetDepot(idSequence[0]).StartTime;
            for (int i = 1; i < idSequence.Length; i++)
            {
                var dist = _problem.GetLocation(idSequence[i]) | _problem.GetLocation(idSequence[i - 1]);
                cost += dist;
                actualTime += TimeSpan.FromMinutes(dist);
                if (_problem.IsClient(idSequence[i]))
                {
                    var c = _problem.GetClient(idSequence[i]);
                    if(c.StartTime>actualTime || c.EndTime<actualTime) return double.MaxValue;
                    actualTime += TimeSpan.FromMinutes(c.Unld);
                    //if(c.StartTime>actualTime || c.EndTime<actualTime) return double.MaxValue; TODO: sprawdzić czy cały rozładunek musi się zmieścić w oknie czasowym
                    load -= c.Size;
                    if (load < 0) return double.MaxValue;
                }
                if(_problem.IsDepot(idSequence[i]))
                {
                    var d = _problem.GetDepot(idSequence[i]);
                    if (d.StartTime > actualTime || d.EndTime < actualTime) return double.MaxValue;
                    load = _problem.Fleet[0].Capacity;
                    //Zakładamy, że pobyt w depocie trwa 0 czasu;
                }
                if (cost > bestCost) return double.MaxValue;
            }
            return cost;
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var permutedClients = Permuter.GeneratePermutations((uint)_problem.Clients.Count());
            //TODO: nie generujemy permutacji różnych długości
            for(int i=0; i<permutedClients.GetLength(0); i++)
                for (int j = 0; j < permutedClients[i].Length; j++)
                {
                    permutedClients[i][j] = _problem.Clients[(int)permutedClients[i][j]].Id;
                }
            // Extra depot means Depot where path cost = 0
            var combinedDepots = Permuter.GenerateCombinations((uint)_problem.Clients.Count() + 1, (uint)_problem.Depots.Count() + 1);
            for (int i = 0; i < combinedDepots.GetLength(0); i++)
            {
                for (int j = 0; j < combinedDepots[i].Length; j++)
                {
                    if (combinedDepots[i][j] == _problem.Depots.Count()) combinedDepots[i][j] = uint.MaxValue;
                    else
                    {
                        combinedDepots[i][j] = _problem.Depots[(int) permutedClients[i][j]].Id;
                    }
                }
            }
            var mixedListClientDepot = new List<List<uint>>();
            for (int i = 0; i < combinedDepots.GetLength(0); i++)
            {
                for (int j = 0; j < permutedClients.GetLength(0); j++)
                {
                    mixedListClientDepot.Add(Combine(combinedDepots[i], permutedClients[j]));
                }
            }
            var helpfulListOfArrays = mixedListClientDepot.Select(lista => lista.ToArray()).ToArray();
            var combinedDestinations = helpfulListOfArrays.ToArray();
            for (int i = 0; i < combinedDestinations.GetLength(0); i++)
            {
                combinedDestinations[i] = combinedDestinations[i].Where(id => id != uint.MaxValue).ToArray();
            }
            var seqCount = (double)combinedDestinations.GetLength(0)/(double)threadCount;
            var actualNodeIndex = 1;
            var sequences = new List<uint[]>();
            var tasksForNodes = new List<byte[]>();
            for (int i = 0; i < combinedDestinations.GetLength(0); i++)
            {
                if (i > actualNodeIndex*seqCount)
                {
                    actualNodeIndex++;
                    var t = new Task(_problem, sequences.ToArray());
                    tasksForNodes.Add(t.Serialize());
                    sequences = new List<uint[]>();
                }
                sequences.Add(combinedDestinations[i]);
            }
            return tasksForNodes.ToArray();
        }

        private List<uint> Combine(uint[] depots, uint[] clients)
        {
            var l = new List<uint>();
            l.Add(depots[0]);
            for (int i = 0; i < clients.Length; i++)
            {
                l.Add(clients[i]);
                l.Add(depots[i + 1]);
            }
            return l;
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
