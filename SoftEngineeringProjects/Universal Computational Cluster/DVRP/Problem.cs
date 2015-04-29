using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DVRP.Objects;

namespace DVRP
{
    [Serializable]
    public class Problem
    {
        // vehicles
        // clients to handle
        // bases

        // Pytanie - to mają być common ^, czy pchamy to wszyskim w ramach jednego problemu ->, a TN ma to puste?
        private int[][] _clientsOrder;

        public Problem(List<Vehicle> fleet, List<Client> clients, List<Depot> depots, int[][] clientsOrder)
        {
            Vehicles = fleet;
            Clients = clients;
            Depots = depots;
            _clientsOrder = clientsOrder;
        }

        public Problem()
        {
        }

        /// Generowane dla kazdego podproblemu
        public List<Client> Clients { get; private set; }

        public List<Depot> Depots { get; private set; }
        public List<Vehicle> Vehicles { get; private set; }

        public Problem[] DivideProblem(int n)
        {
            var subproblems = new Problem[n];
            for (var i = 0; i < n; i++)
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

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }

        public static Problem Deserialize(byte[] byteArray)
        {
            return (Problem) Serializer.Deserialize(byteArray);
        }

        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}