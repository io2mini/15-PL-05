using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using DVRP.Objects;

namespace DVRP
{
    [Serializable]
    public class Problem
    {
        private List<Vehicle> _fleet;
        private List<Client> _clients;
        private List<Depot> _depots;
        
        // Pytanie - to mają być common ^, czy pchamy to wszyskim w ramach jednego problemu ->, a TN ma to puste?
        private int[][] _clientsOrder; /// Generowane dla kazdego podproblemu

        public Problem[] DivideProblem(int n)
        {
            Problem[] subproblems = new Problem[n];
            for (int i = 0; i < n; i++)
            {
                subproblems[i] = new Problem();
            }

            // Generuj permutacje


            throw new NotImplementedException();
        }

        public Problem MergeProblem(Problem[] problems)
        {
            throw new NotImplementedException();
        }
    }
}