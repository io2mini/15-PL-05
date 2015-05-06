using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using DVRP.Objects;

namespace DVRP
{
    public static class DVRPExtensions
    {
        public static double CalculteRouteCost(this Route r, Problem p, double bestCost = double.MaxValue)
        {
            //bool s1 = p.Depots.Aggregate(false, (current, depot) => current || depot.Id == r.Sequence[0]);
            //bool s2 = p.Depots.Aggregate(false, (current, depot) => current || depot.Id == r.Sequence[r.Sequence.Length - 1]);
            //if (!s1 || !s2) return double.MaxValue; //Żaden z końców sekwencji nie jest depotem
            //Powyższe nie wystąpi;
            double cost = 0;
            double load = p.Fleet[0].Capacity;
            var actualTime = p.GetDepot(r.Sequence[0]).StartTime; //TODO: dodać czekanie na otwarcie domu/depotu?
            for (int i = 1; i < r.Sequence.Length; i++)
            {
                var dist = p.GetLocation(r.Sequence[i]) | p.GetLocation(r.Sequence[i - 1]);
                cost += dist;
                actualTime += TimeSpan.FromMinutes(dist);
                if (p.IsClient(r.Sequence[i]))
                {
                    var c = p.GetClient(r.Sequence[i]);
                    //if(c.StartTime>actualTime || c.EndTime<actualTime) return double.MaxValue; //wersja bez czekania

                        //Wersja z czekaniem na otwarcie:
                        if (c.EndTime < actualTime) return double.MaxValue;
                        if (c.StartTime > actualTime) actualTime = c.StartTime;
                        //koniec wersji z czekaniem na otwarcie

                    actualTime += TimeSpan.FromMinutes(c.Unld);
                    //if(c.StartTime>actualTime || c.EndTime<actualTime) return double.MaxValue; TODO: sprawdzić czy cały rozładunek musi się zmieścić w oknie czasowym
                    load -= c.Size;
                    if (load < 0) return double.MaxValue;
                }
                if(p.IsDepot(r.Sequence[i]))
                {
                    var d = p.GetDepot(r.Sequence[i]);
                    //if(d.StartTime>actualTime || d.EndTime<actualTime) return double.MaxValue; //wersja bez czekania

                        //Wersja z czekaniem na otwarcie:
                        if (d.EndTime < actualTime) return double.MaxValue;
                        if (d.StartTime > actualTime) actualTime = d.StartTime;
                        //koniec wersji z czekaniem na otwarcie

                    load = p.Fleet[0].Capacity;
                    //Zakładamy, że pobyt w depocie trwa 0 czasu;
                }
                if (cost > bestCost) return double.MaxValue;
            }
            return cost;
        }

        private static Route AddDepotsToRoute(this Route r, uint[] depotSequence)
        {
            var l = new List<uint> {depotSequence[0]};
            for(int i=0; i<r.Sequence.Length; i++)
            {
                l.Add(r.Sequence[i]);
                l.Add(depotSequence[i + 1]);
            }
            //zwrócenie listy z usuniętym "sztucznym" depotem (oznaczającym "nie jedź do depotu")
            return new Route(l.Where(id => id != uint.MaxValue).ToArray());
        }

        public static Route[] GenerateAndAddDepotsToRoute(this Route r, Problem p, List<uint[][]> l)
        {
            //Generowanie kombinacji depotów i konwersja ich indeksów na id
            var combinedDepots = l[r.Sequence.Length + 1];//AssignDepotIds(Permuter.GenerateCombinations((uint)r.Sequence.Length + 1, (uint)p.Depots.Count() + 1), p);
            //Usuwanie kombinacji zaczynających się lub kończących się na "sztucznym" depocie (oznaczającym "nie jedź do depotu")
            //combinedDepots = combinedDepots.Where(depotArray => depotArray.First() != uint.MaxValue && depotArray.Last() != uint.MaxValue).ToArray();
            //Dodanie depotów do trasy
            return combinedDepots.Select(depotSequence => r.AddDepotsToRoute(depotSequence)).ToArray();
        }

        private static uint[][] AssignDepotIds(uint[][] tab, Problem p)
        {
            for (int i = 0; i < tab.GetLength(0); i++)
            {
                for (int j = 0; j < tab[i].Length; j++)
                {
                    tab[i][j] = tab[i][j] == p.Depots.Count() ? uint.MaxValue : p.Depots[(int) tab[i][j]].Id;
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

        public TaskSolver(byte[] problemData) : base(problemData)
        {
             ProblemInstance = Problem.Deserialize(problemData);
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            //TODO: handle timeout
            var task = Task.Deserialize(partialData);
            double cost = double.MaxValue;
            Route[] bestSequence = null;
            foreach (var sequence in task.Routes)
            {
                var costPrim = sequence.Sum(route => route.CalculteRouteCost(ProblemInstance, cost));
                if (costPrim < cost)
                {
                    cost = costPrim;
                    bestSequence = sequence;
                }
            }
            var s = new Solution(bestSequence != null ? bestSequence.ToList() : null);
            return s.Serialize();
        }

        public double CountSigleVehicleCost(Route r, double bestCost)
        {
            return r.CalculteRouteCost(ProblemInstance, bestCost);
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var permutedClients = AssignClientIds(Permuter.GeneratePermutations((uint)ProblemInstance.Clients.Count()));
            var divides = Permuter.GenerateBooleanCombination((uint)ProblemInstance.Clients.Count, (uint)ProblemInstance.Vehicles.Count);
            var RouteClients = DivideClients(permutedClients, divides).ToList();
            var combinedDestinations = new List<Route[]>();

            var list =new List<uint[][]>();
            list.Add(null); //wartownik
            for (int i = 1; i < ProblemInstance.Clients.Count + 2; i++)
            {
                list.Add(AssignDepotIds(
                    Permuter.GenerateCombinations((uint) i, (uint) ProblemInstance.Depots.Count() + 1)));
                list[i] = list[i].Where(depotArray => depotArray.First() != uint.MaxValue && depotArray.Last() != uint.MaxValue).ToArray();
            }

            combinedDestinations = RouteClients.Select(routes => routes.Select(r => r.GenerateAndAddDepotsToRoute(ProblemInstance, list)).Select(lll => lll.ToArray()).ToList()).Aggregate(combinedDestinations, (current, l) => current.Union(l).ToList());
            //Powyższe wygenerowane resharperem z poniższego gdzie combinedDestinations to ll:
            //foreach (Route[] routes in RouteClients)
            //{
            //    var l = new List<Route[]>();
            //    foreach (Route r in routes)
            //    {
            //        var lll = r.GenerateAndAddDepotsToRoute(_problem);
            //        l.Add(lll.ToArray());
            //    }
            //    ll = ll.Union(l).ToList();
            //}

            // Extra depot means Depot where path cost = 0
            // Dzielenie na taski
            var seqCount = (double)combinedDestinations.Count/(double)threadCount;
            var actualNodeIndex = 1;
            var sequences = new List<Route[]>();
            var tasksForNodes = new List<byte[]>();
            tasksForNodes.Add(ProblemInstance.Serialize()); // Common data przechowywane w zerowym indeksie
            for (int i = 0; i < combinedDestinations.Count; i++)
            {
                if (i > actualNodeIndex*seqCount)
                {
                    actualNodeIndex++;
                    var t = new Task(sequences.ToArray());
                    tasksForNodes.Add(t.Serialize());
                    sequences.Clear();
                }
                sequences.Add(combinedDestinations[i]);
            }
            return tasksForNodes.ToArray();
        }

        private Route[][] DivideClients(uint[][] permutedClients, bool[][] divides)
        {
            List<Route[]> l = new List<Route[]>();
            for (int i = 0; i < permutedClients.GetLength(0); i++)
            {
                for (int j = 0; j < divides.GetLength(0); j++)
                {
                    l.Add(CombineClientDivide(permutedClients[i], divides[j]));
                }
            }
            return l.ToArray();
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
            for (int i = 0; i < p.GetLength(0); i++)
                for (int j = 0; j < p[i].Length; j++)
                {
                    p[i][j] = ProblemInstance.Clients[(int)p[i][j]].Id;
                }
            return p;
        }

        private List<uint> Combine(uint[] depots, uint[] clients)
        {
            var l = new List<uint>();
            l.Add(depots[0]);
            for (int i = 0; i < clients.Length; i++)
            {
                l.Add(clients[i]);
                l.Add(depots[i+1]);
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
            get { return "DVRP"; }
        }
    }
}
