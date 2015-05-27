using System;
using System.Collections;
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
        public double CutOff { get; set; }
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

        public Problem(List<Vehicle> fleet, List<Client> clients, List<Depot> depots, int[][] clientsOrder,double cutOff =0.5)
        {
            Fleet = fleet;
            Clients = clients;
            Depots = depots;
            _clientsOrder = clientsOrder;
            CutOff = cutOff;
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
            string[] problemFileLines = File.ReadAllLines(fileUri.IsAbsoluteUri?fileUri.AbsolutePath:fileUri.LocalPath);

            Dictionary<uint, Location> locations = new Dictionary<uint, Location>();
            Dictionary<uint, double> demands = new Dictionary<uint, double>();
            List<uint> depotsIds = new List<uint>();
            List<uint> clientsIds = new List<uint>();
            //Dictionary<uint, >
            Dictionary<uint, Tuple<TimeSpan, TimeSpan>> depotTimesWindows = new Dictionary<uint, Tuple<TimeSpan, TimeSpan>>();
            Dictionary<uint, TimeSpan> timesAvivals = new Dictionary<uint, TimeSpan>();
            Dictionary<uint, double> unld = new Dictionary<uint, double>();
            double cutoff = 0.5;
            int numberOfVehicles = 0;
            int vehiclesCapaticies = 0;

            //string name;
            //int num_depots, num_capacities, num_visits, num_locations, num_vehicles, capacities;

            for (int i = 0; i < problemFileLines.Length; i++)
            {
                switch (problemFileLines[i])
                {
                    case "DEPOTS":
                        do
                        {
                            i++;
                            depotsIds.Add(Convert.ToUInt32(problemFileLines[i].Trim()));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "DEMAND_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            clientsIds.Add(Convert.ToUInt32(line[0]));
                            demands.Add(Convert.ToUInt32(line[0]), Convert.ToDouble(line[1]));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "LOCATION_COORD_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            locations.Add(Convert.ToUInt32(line[0]), new Location(Convert.ToDouble(line[1]),Convert.ToDouble(line[2])));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "DURATION_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            unld.Add(Convert.ToUInt32(line[0]), Convert.ToDouble(line[1]));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "DEPOT_TIME_WINDOW_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            depotTimesWindows.Add(Convert.ToUInt32(line[0]), new Tuple<TimeSpan, TimeSpan>(new TimeSpan(0,Convert.ToInt32(line[1]),0), new TimeSpan(0,Convert.ToInt32(line[2]),0)));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "TIME_AVAIL_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            timesAvivals.Add(Convert.ToUInt32(line[0]), new TimeSpan(0,Convert.ToInt32(line[1]),0));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                }

                string[] divLine = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                switch (divLine[0])
                {
                    case "NUM_VEHICLES:":
                        numberOfVehicles = Convert.ToInt32(divLine[1]);
                        break;
                    case "CAPACITIES:":
                        vehiclesCapaticies = Convert.ToInt32(divLine[1]);
                        break;
                }
            }

            List<Depot> depots = new List<Depot>();
            List<Vehicle> vehicles = new List<Vehicle>();
            List<Client> clients = new List<Client>();

            for (int i = 0; i < depotsIds.Count; i++)
            {
                depots.Add(new Depot(locations[depotsIds[i]], depotTimesWindows[depotsIds[i]].Item1, depotTimesWindows[depotsIds[i]].Item2, depotsIds[i]));
            }
            var maxEnd = depots.Max<Depot>((depot => (depot.EndTime.Ticks)));
         
            for (int i = 0; i < clientsIds.Count; i++)
            {
                clients.Add(new Client(locations[clientsIds[i]], timesAvivals[clientsIds[i]], new TimeSpan(maxEnd), unld[clientsIds[i]], demands[clientsIds[i]], clientsIds[i]));

            }
            foreach (var C in clients)
            {
              
                
                var T = new TimeSpan((long) (cutoff*(double) maxEnd));
                if (C.StartTime >= T)
                {
                
                    C.CutOff = true;
                }
            }
            for (int i = 0; i < numberOfVehicles; i++)
            {
                vehicles.Add(new Vehicle(new Location(0, 0), vehiclesCapaticies, 1));
            }

            return new Problem(vehicles, clients, depots, null,cutoff);
        }
    }
}