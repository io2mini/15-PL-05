using System;
using System.Collections.Generic;
using DVRP.Objects;

namespace DVRP
{
    [Serializable]
    public class Problem
    {
        private List<Vehicle> _fleet;
        private List<Client> _clients;
        private List<Depot> _depots;

        public Problem[] DivideProblem(int n)
        {
            throw new NotImplementedException();
        }

        public Problem MergeProblem(Problem[] problems)
        {
            throw new NotImplementedException();
        }
    }
}