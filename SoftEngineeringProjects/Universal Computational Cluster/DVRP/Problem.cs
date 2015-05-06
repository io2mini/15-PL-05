using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DVRP.Objects;

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


        private static readonly char[] WHITESPACES = { ' ', '\t', '\n' };
        public static Problem CreateProblemInstanceFromFile(Uri fileUri)
        {
            // Oczytaj parametry z pliku
            string[] problemFileLines = File.ReadAllLines(fileUri.AbsolutePath);

            Dictionary<uint, Location> locations = new Dictionary<uint, Location>();
            Dictionary<uint, double> demands = new Dictionary<uint, double>();
            List<uint> depots = new List<uint>();

            string name;
            int num_depots, num_capacities, num_visits, num_locations, num_vehicles, capacities;

            for (int i = 0; i < problemFileLines.Length; i++)
            {
                switch (problemFileLines[i])
                {
                    case "DEPOTS":
                        do
                        {
                            i++;
                            depots.Add(Convert.ToUInt32(problemFileLines[i].Trim()));
                        } while (!problemFileLines[i + 1].Equals("DEMAND_SECTION"));
                        break;
                    case "DEMAND_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            uint id = Convert.ToUInt32(line[0]);
                            demands.Add(id, Convert.ToDouble(line[1]));
                        } while (!problemFileLines[i + 1].Equals("LOCATION_COORD_SECTION"));
                        break;
                    case "LOCATION_COORD_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            locations.Add(Convert.ToUInt32(line[0]), new Location(Convert.ToDouble(line[1]),Convert.ToDouble([line[2]]));
                        } while (!problemFileLines[i + 1].Equals("DEPOT_LOCATION_SECTION"));
                        break;
                }
            }

            return new Problem();
        }
    }
}