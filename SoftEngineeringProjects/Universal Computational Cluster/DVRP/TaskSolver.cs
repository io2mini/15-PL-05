﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;

namespace DVRP
{
    public class TaskSolver : UCCTaskSolver.TaskSolver
    {
        private readonly Problem _problem;

        public TaskSolver(byte[] problemData)
            : base(problemData)
        {
            _problem = Problem.Deserialize(problemData);
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] Solve(byte[] partialData, TimeSpan timeout)
        {
            var task = Task.Deserialize(partialData);
            return null;
        }

        public override byte[][] DivideProblem(int threadCount)
        {
            var permutedClients = Permuter.GeneratePermutations((uint) _problem.Clients.Count());
            for (var i = 0; i < permutedClients.GetLength(0); i++)
                for (var j = 0; j < permutedClients[i].Length; j++)
                {
                    permutedClients[i][j] = _problem.Clients[(int) permutedClients[i][j]].Id;
                }
            // Extra depot means Depot where path cost = 0
            var combinedDepots = Permuter.GenerateCombinations((uint) _problem.Clients.Count() + 1,
                (uint) _problem.Depots.Count() + 1);
            for (var i = 0; i < combinedDepots.GetLength(0); i++)
            {
                for (var j = 0; j < combinedDepots[i].Length; j++)
                {
                    if (combinedDepots[i][j] == _problem.Depots.Count()) combinedDepots[i][j] = uint.MaxValue;
                    else
                    {
                        combinedDepots[i][j] = _problem.Depots[(int) permutedClients[i][j]].Id;
                    }
                }
            }
            var mixedListClientDepot = new List<List<uint>>();
            for (var i = 0; i < combinedDepots.GetLength(0); i++)
            {
                for (var j = 0; j < permutedClients.GetLength(0); j++)
                {
                    mixedListClientDepot.Add(Combine(combinedDepots[i], permutedClients[j]));
                }
            }
            var helpfulListOfArrays = mixedListClientDepot.Select(lista => lista.ToArray()).ToArray();
            var combinedDestinations = helpfulListOfArrays.ToArray();
            for (var i = 0; i < combinedDestinations.GetLength(0); i++)
            {
                combinedDestinations[i] = combinedDestinations[i].Where(id => id != uint.MaxValue).ToArray();
            }
            var seqCount = combinedDestinations.GetLength(0)/(double) threadCount;
            var actualNodeIndex = 1;
            var sequences = new List<uint[]>();
            var tasksForNodes = new List<byte[]>();
            for (var i = 0; i < combinedDestinations.GetLength(0); i++)
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
            for (var i = 0; i < clients.Length; i++)
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
            var cost = double.MaxValue;
            foreach (var byteArray in byteSolutions)
            {
                var solution_prim = Solution.Deserialize(byteArray);
                if (solution_prim.Cost < cost)
                {
                    cost = solution_prim.Cost;
                    solution = solution_prim;
                }
            }
            if (solution == null) throw new SolutionNotFoundException();
            return solution.Serialize();
        }
    }
}