using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using DVRP.Objects;

namespace DVRP
{
    public static class DVRPExtensions
    {
        public static Tuple<uint[], double> CalculteRouteCost(this Route r, Problem p, double bestCost = double.MaxValue)
        {
            var failed = new Tuple<uint[], double>(null, double.MaxValue);
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
            var visitList = new List<uint> {startDepot.Id};
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
                if (i < r.Sequence.Length-1 && r.DepotAfter[i])
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

        private static Route AddDepotsToRoute(this Route r, bool[] depotSequence)
        {
            return new Route(r.Sequence, depotSequence);
        }

        

        private static uint[][] AssignDepotIds(uint[][] tab, Problem p)
        {
            for (int i = 0; i < tab.GetLength(0); i++)
            {
                for (int j = 0; j < tab[i].Length; j++)
                {
                    tab[i][j] = tab[i][j] == p.Depots.Count() ? uint.MaxValue : p.Depots[(int)tab[i][j]].Id;
                }
            }
            return tab;
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
            var routes = GeneratePermutedClients(task.Brackets);
           
           
            double bestCost = double.MaxValue;
            uint[][] bestSequence = null;
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
                if(actCost>=bestcost) return Double.MaxValue;
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
            var seqCount = ((double)brackets.GetLength(0) / (double)threadCount);
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
        public List<Route[]> GeneratePermutedClients(int[][] brackets)
        {
            var permutedClients = Permuter.GenerateAndFillBrackets(ProblemInstance.Vehicles.Count,
                ProblemInstance.Clients.Count, brackets);
            var RouteClients = DivideClients(permutedClients).ToList();
            var combinedDestinations = new List<Route[]>();
            var maxClientCount = permutedClients.Aggregate(0,
                (current, combination) => combination.Select(routeSeq => routeSeq.Length).Concat(new[] { current }).Max());
            var booleanCombinations = new List<bool[]> { new bool[] { } };
            for (int i = 1; i < maxClientCount; i++)
            {
                var predecessors = booleanCombinations.Where(b => b.Length == i).ToList();
                foreach (bool[] predecessor in predecessors)
                {
                    var l1 = predecessor.ToList();
                    l1.Add(false);
                    booleanCombinations.Add(l1.ToArray());
                    var l2 = predecessor.ToList();
                    l2.Add(true);
                    booleanCombinations.Add(l2.ToArray());
                }
            }
            var list = new List<uint[][]> { null };
            //wartownik
            for (int i = 1; i < ProblemInstance.Clients.Count + 2; i++)
            {
                list.Add(AssignDepotIds(
                    Permuter.GenerateCombinations((uint)i, (uint)ProblemInstance.Depots.Count() + 1)));
                list[i] = list[i].Where(depotArray => depotArray.First() != uint.MaxValue && depotArray.Last() != uint.MaxValue).ToArray();
            }

            // combinedDestinations = RouteClients.Select(routes => routes.Select(r => r.GenerateAndAddDepotsToRoute(ProblemInstance, list)).Select(lll => lll.ToArray()).ToList()).Aggregate(combinedDestinations, (current, l) => current.Union(l).ToList());
            //Powyższe wygenerowane resharperem z poniższego gdzie combinedDestinations to ll:
            foreach (Route[] routes in RouteClients)
            {
                List<Route[]> current = new List<Route[]>();
                var l = new List<Route>();
                foreach (Route r in routes)
                {
                    //var lll = r.GenerateAndAddDepotsToRoute(ProblemInstance, list);
                    //current = CombineRoutes(current, lll);
                }
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
            return permutedClients.Select(array => AssignClientIds(array)).Select(A => A.Select(ar2 => new Route(ar2)).ToArray()).ToArray();
        }

        private Route[] CombineClientDivide(uint[] permutedClient, bool[] bools)
        {
            var Result = new List<Route>();
            var singleRoute = new List<uint>();
            for (int i = 0; i < permutedClient.Length; i++)
            {
                singleRoute.Add(permutedClient[i]);
                if (bools[i] || i == permutedClient.Length - 1)
                {
                    Result.Add(new Route(singleRoute.ToArray()));
                    singleRoute.Clear();
                }
            }
            return Result.ToArray();
        }

        private uint[][] CombineClientsWithDepots(uint[][] permutedClients, uint[][] combinedDepots)
        {
            var resultList = new List<List<uint>>();
            for (int i = 0; i < combinedDepots.GetLength(0); i++)
            {
                for (int j = 0; j < permutedClients.GetLength(0); j++)
                {
                    resultList.Add(Combine(combinedDepots[i], permutedClients[j]));
                }
            }
            var helpfulListOfArrays = resultList.Select(lista => lista.ToArray()).ToArray();
            var combinedDestinations = helpfulListOfArrays.ToArray();
            for (int i = 0; i < combinedDestinations.GetLength(0); i++)
            {
                combinedDestinations[i] = combinedDestinations[i].Where(id => id != uint.MaxValue).ToArray();
            }
            return combinedDestinations;
        }

        private uint[][] AssignDepotIds(uint[][] p)
        {
            for (int i = 0; i < p.GetLength(0); i++)
            {
                for (int j = 0; j < p[i].Length; j++)
                {
                    if (p[i][j] == ProblemInstance.Depots.Count()) p[i][j] = uint.MaxValue;
                    else
                    {
                        p[i][j] = ProblemInstance.Depots[(int)p[i][j]].Id;
                    }
                }
            }
            return p;
        }

        private uint[][] AssignClientIds(uint[][] p)
        {
            uint[][] result = new uint[p.GetLength(0)][];
            for (int i = 0; i < p.GetLength(0); i++)
            {
                result[i] = new uint[p[i].Length];
                for (int j = 0; j < p[i].Length; j++)
                {
                    int t = (int)p[i][j];

                    result[i][j] = ProblemInstance.Clients[(int)p[i][j]].Id;
                }

            }
            return result;
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
