using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using DVRP.Objects;

namespace DVRP
{
    // ReSharper disable once InconsistentNaming
    public static class DVRPExtensions
    {
        public static Tuple<uint[], double> CalculteRouteCost(this Route r, Problem p, double bestCost = Double.MaxValue)
        {
            var failed = new Tuple<uint[], double>(null, Double.MaxValue);
            //bool s1 = p.Depots.Aggregate(false, (current, depot) => current || depot.Id == r.Sequence[0]);
            //bool s2 = p.Depots.Aggregate(false, (current, depot) => current || depot.Id == r.Sequence[r.Sequence.Length - 1]);
            //if (!s1 || !s2) return double.MaxValue; //Żaden z końców sekwencji nie jest depotem
            //Powyższe nie wystąpi;
            double cost = 0;
            double load = p.Fleet[0].Capacity;
            var startDepot = p.GetClosestValidStartingDepotToClient(r.Sequence[0]);
            if (startDepot == null) return failed;
            var actualTime = startDepot.StartTime;
            var prevLocation = startDepot.Location;
            var visitList = new List<uint> { startDepot.Id };
            for (int i = 0; i < r.Sequence.Length; i++)
            {
                var actClientId = r.Sequence[i];
                var actClient = p.GetClient(actClientId);
                var dist = prevLocation | actClient.Location;
                cost += dist;
                if (cost >= bestCost) return failed;
                actualTime += TimeSpan.FromMinutes(dist);
                actualTime += TimeSpan.FromMinutes(actClient.Unld);
                load += actClient.Size;
                if (actClient.RealStartTime > actualTime || actClient.EndTime < actualTime || load < 0)
                {
                    return failed;
                }
                prevLocation = actClient.Location;
                visitList.Add(actClient.Id);
                if (i < r.Sequence.Length - 1 && r.DepotAfter[i])
                {
                    var nextClient = p.GetClient(r.Sequence[i + 1]);
                    var depot = p.GetClosestValidDepot(actClientId, nextClient.Id, actualTime);
                    if (depot == null) return failed;
                    cost += (actClient.Location | depot.Location);
                    prevLocation = depot.Location;
                    load = p.Fleet[0].Capacity;
                    visitList.Add(depot.Id);
                }
            }
            var lastClient = p.GetClient(visitList.Last());
            var lastDepot = p.GetClosestValidDepot(lastClient.Id, actualTime);
            if (lastDepot == null) return failed;
            cost += lastDepot.Location | lastClient.Location;
            visitList.Add(lastDepot.Id);
            return new Tuple<uint[], double>(visitList.ToArray(), cost);
        }

        internal static Route AddDepotsToRoute(this Route r, bool[] depotSequence)
        {
            return new Route(r.Sequence, depotSequence);
            
        }

        public static Route[] GenerateAndAddDepotsToRoute(this Route r, Problem p, bool[][] l)
        {
            int minDepotCount = (int)Math.Abs(r.Sequence.Sum(s => (p.GetClient(s).Size)));
            minDepotCount /= p.Fleet[0].Capacity;
            //Generowanie kombinacji depotów i konwersja ich indeksów na id
            //AssignDepotIds(Permuter.GenerateCombinations((uint)r.Sequence.Length + 1, (uint)p.Depots.Count() + 1), p);
            //Usuwanie kombinacji zaczynających się lub kończących się na "sztucznym" depocie (oznaczającym "nie jedź do depotu")
            //combinedDepots = combinedDepots.Where(depotArray => depotArray.First() != uint.MaxValue && depotArray.Last() != uint.MaxValue).ToArray();
            //Dodanie depotów do trasy

            //return l.Where(o => (o.Sum(d => (d ? 1 : 0)) >= minDepotCount - 1)).Select(r.AddDepotsToRoute).ToArray();
            //usprawnienie: wybrać najoptymalniejszy Route tutaj zamiast generować i przekazywać dalej
            double Min = double.MaxValue;
            Route best = null;
            foreach (var array in l)
            {
                r.DepotAfter = array;
                var current = r.CalculteRouteCost(p, Min);
                if (current.Item2 < Min)
                {
                    best = r.AddDepotsToRoute(array);
                    Min = current.Item2;
                }

            }
            return best==null?new Route[]{r}:new[] { best };
        }

        
    }

    public class TaskSolver : UCCTaskSolver.TaskSolver
    {

        public static TaskSolver TaskSolverFactory(byte[] problemData)
        {
            return new TaskSolver(problemData);
        }

        public Problem ProblemInstance { get; private set; }

        public TaskSolver(byte[] problemData)
            : base(problemData)
        {
            ProblemInstance = Problem.Deserialize(problemData);
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            //TODO: handle timeout

            var task = Task.Deserialize(partialData);
             uint[][] bestSequence = null;
             double bestCost = double.MaxValue;

             var booleanCombinations = new List<bool[]> { new bool[] { } };
             for (var i = 0; i < ProblemInstance.Clients.Count; i++)
             {
                 var predecessors = booleanCombinations.Where(b => b.Length == i).ToList();
                 foreach (bool[] predecessor in predecessors)
                 {
                     var l1 = predecessor.ToList();
                     l1.Add(false);
                     booleanCombinations.Add(l1.ToArray());
                     l1 = predecessor.ToList();
                     l1.Add(true);
                     booleanCombinations.Add(l1.ToArray());
                 }
             }
            foreach (var array in task.Brackets)
            {
                GC.Collect();
                var routes = GeneratePermutedClients(new [] {array},booleanCombinations);
                foreach (var sequence in routes)
                {
                    uint[][] seq;
                    var currentCost = CalculateResult(sequence, out seq, bestCost);
                    if (currentCost < bestCost)
                    {
                        bestCost = currentCost;
                        bestSequence = seq;
                    }
                }
            }
            var s = new Solution(bestSequence != null ? bestSequence.ToList() : null, bestCost);
            return s.Serialize();
        }

        private double CalculateResult(Route[] sequence, out uint[][] seq, double bestcost)
        {
            var l = new List<uint[]>();
            double actCost = 0;
            seq = null;
            foreach (var route in sequence)
            {
                var res = route.CalculteRouteCost(ProblemInstance, bestcost);
                actCost += res.Item2;
                if (actCost >= bestcost) return Double.MaxValue;
                l.Add(res.Item1);
            }
            seq = l.ToArray();
            return actCost;
        }

        public double CountSigleVehicleCost(Route r, double bestCost)
        {
            return r.CalculteRouteCost(ProblemInstance, bestCost).Item2;
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            ProblemInstance.GenerateDepotDistances();
            var brackets = Permuter.GenerateLengthBrackets(ProblemInstance.Vehicles.Count, ProblemInstance.Clients.Count);
            var seqCount = (brackets.GetLength(0) / (double)threadCount);
            var tasksForNodes = new List<byte[]> { ProblemInstance.Serialize() };
            // Common data przechowywane w zerowym indeksie
            var actualNodeIndex = 1f;
            var sequences = new List<int[]>();
            for (int i = 0; i < brackets.GetLength(0); i++)
            {
                sequences.Add(brackets[i]);
                if (i == (int)((actualNodeIndex * seqCount) - 1))
                {
                    actualNodeIndex++;
                    var t = new Task(sequences.ToArray());
                    tasksForNodes.Add(t.Serialize());
                    sequences.Clear();
                }

            }
            return tasksForNodes.ToArray();
        }

        public List<Route[]> GeneratePermutedClients(int[][] brackets,List<bool[]> booleanCombinations)
        {
            var permutedClients = Permuter.GenerateAndFillBrackets(ProblemInstance.Vehicles.Count,
                ProblemInstance.Clients.Count, brackets);
            var routeClients = DivideClients(permutedClients).ToList();
            var combinedDestinations = new List<Route[]>();
           

            // combinedDestinations = RouteClients.Select(routes => routes.Select(r => r.GenerateAndAddDepotsToRoute(ProblemInstance, list)).Select(lll => lll.ToArray()).ToList()).Aggregate(combinedDestinations, (current, l) => current.Union(l).ToList());
            //Powyższe wygenerowane resharperem z poniższego gdzie combinedDestinations to ll:
            foreach (var current in from routes in routeClients let current = new List<Route[]>() select routes.Select(r => r.GenerateAndAddDepotsToRoute(ProblemInstance, booleanCombinations.Where(arr => arr.Length == r.Sequence.Length - 1).ToArray())).Aggregate(current, CombineRoutes))
            {
                combinedDestinations.AddRange(current);
            }

            // Extra depot means Depot where path cost = 0
            // Dzielenie na taski
            return combinedDestinations;
        }

        private List<Route[]> CombineRoutes(List<Route[]> current, Route[] toAdd)
        {
            List<Route[]> target = new List<Route[]>();

            foreach (var r in current)
            {

                foreach (var tA in toAdd)
                {
                    var r1 = new List<Route>();
                    r1.AddRange(r);
                    r1.Add(tA);
                    target.Add(r1.ToArray());
                }

            }
            if (current.Count == 0)
            {
                target.AddRange(toAdd.Select(tA => new List<Route> { tA }).Select(r1 => r1.ToArray()));
            }
            return target;
        }
        private Route[][] DivideClients(List<uint[][]> permutedClients)
        {
            return permutedClients.Select(AssignClientIds).Select(a => a.Select(ar2 => new Route(ar2)).ToArray()).ToArray();
        }

        


        private uint[][] AssignClientIds(uint[][] p)
        {
            uint[][] result = new uint[p.GetLength(0)][];
            for (int i = 0; i < p.GetLength(0); i++)
            {
                result[i] = new uint[p[i].Length];
                for (int j = 0; j < p[i].Length; j++)
                {
                    

                    result[i][j] = ProblemInstance.Clients[(int)p[i][j]].Id;
                }

            }
            return result;
        }

    
        public override byte[] MergeSolution(byte[][] byteSolutions)
        {
            if (byteSolutions == null) throw new ArgumentNullException();
            Solution solution = null;
            double cost = Double.MaxValue;
            foreach (byte[] byteArray in byteSolutions)
            {
                Solution solutionPrim = Solution.Deserialize(byteArray);
                if (solutionPrim.Cost < cost)
                {
                    cost = solutionPrim.Cost;
                    solution = solutionPrim;
                }
            }
            if (solution == null) throw new SolutionNotFoundException();
            return solution.Serialize();
        }

        public override string Name
        {
            get { return "DVRP"; }
        }
    }
}
