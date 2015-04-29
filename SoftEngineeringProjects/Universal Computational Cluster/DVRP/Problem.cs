using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using DVRP.Objects;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

namespace DVRP
{
    [Serializable]
    public class Problem
    {
        // vehicles
        public List<Vehicle> Fleet { get; private set; }
        // clients to handle
        public List<Client> Clients { get; private set; }
        // bases
        public List<Depot> Depots { get; private set; }

        // Pytanie - to mają być common ^, czy pchamy to wszyskim w ramach jednego problemu ->, a TN ma to puste?
        private int[][] _clientsOrder; /// Generowane dla kazdego podproblemu
        /// 
        //public List<Client> Clients { get { return _clients; } }
        //public List<Depot> Depots { get { return _depots; } }
        public List<Vehicle> Vehicles { get { return Fleet; } }

        public Location GetLocation(uint id)
        {
            var l = Clients.Select(client => new Tuple<uint, Location>(client.Id,client.Location)).Union(Depots.Select(depot => new Tuple<uint, Location>(depot.Id, depot.Location))).ToList();
            foreach (var location in l.Where(location => location.Item1 == id))
            {
                return location.Item2;
            }
            throw new KeyNotFoundException(); 
        }

        public bool IsDepot(uint id)
        {
            return Depots.Select(depot => depot.Id).Contains(id);
        }

        public Depot GetDepot(uint id)
        {
            foreach (var depot in Depots.Where(depot => depot.Id == id))
            {
                return depot;
            }
            throw new KeyNotFoundException();
        }

        public bool IsClient(uint id)
        {
            return Clients.Select(client => client.Id).Contains(id);
        }

        public Client GetClient(uint id)
        {
            foreach (var client in Clients.Where(depot => depot.Id == id))
            {
                return client;
            }
            throw new KeyNotFoundException();
        }

        public Problem(List<Vehicle> fleet, List<Client> clients, List<Depot> depots, int[][] clientsOrder)
        {
            Fleet = fleet;
            Clients = clients;
            Depots = depots;
            _clientsOrder = clientsOrder;
        }

        public Problem()
        {

        }

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

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }

        public static Problem Deserialize(byte[] byteArray)
        {
            return (Problem)Serializer.Deserialize(byteArray);
        }

        private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}