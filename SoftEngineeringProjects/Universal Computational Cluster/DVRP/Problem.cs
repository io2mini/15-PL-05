using System;
using System.CodeDom.Compiler;
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

        private List<int>[,] _depotDistances;

        /// <summary>
        /// Inicjalizuje pole, potrzebne do wykonywania funkcji znajdujących optymalny depot
        /// </summary>
        public void GenerateDepotDistances()
        {
            var n = Clients.Count;
            _depotDistances = new List<int>[n,n];
            for(int i=0; i < n; i++)
                for (int j = i; j < n; j++)
                {
                    _depotDistances[i,j] = new List<int>();
                    for (int k = 0; k < Depots.Count; k++)
                    {
                        _depotDistances[i,j].Add(k);
                    }
                    var j1 = j;
                    var i1 = i;
                    _depotDistances[i, j].Sort((x, y) =>
                    {
                        var px = Depots[x].Location;
                        var py = Depots[y].Location;
                        var distanceX = (Clients[i1].Location | px) + (px | Clients[j1].Location);
                        var distanceY = (Clients[i1].Location | py) + (py | Clients[j1].Location);
                        var com = distanceX.CompareTo(distanceY);
                        return com == 0 ? Depots[x].StartTime.CompareTo(Depots[y].StartTime) : com;
                    });
                    _depotDistances[j, i] = _depotDistances[i, j];
                }
        }

        /// <summary>
        /// Znajduje listę depotów posortowaną względem odległości od klienta
        /// </summary>
        /// <param name="id">ID klienta</param>
        /// <returns></returns>
        public List<Depot> GetDepotList(uint id)
        {
            return GetDepotList(id, id);
        }

        /// <summary>
        /// Znajduje listę depotów posortowaną względem sumy odległości od klientów
        /// </summary>
        /// <param name="clientId1">ID klienta z którego wyjeżdżamy</param>
        /// <param name="clientId2">ID klienta do którego docieramy</param>
        /// <returns></returns>
        public List<Depot> GetDepotList(uint clientId1, uint clientId2)
        {
            if(!IsClient(clientId1) || !IsClient(clientId2)) throw new ArgumentException();
            var ind1 = Clients.IndexOf(GetClient(clientId1));
            var ind2 = Clients.IndexOf(GetClient(clientId2));
            var indList = _depotDistances[ind1, ind2];
            return indList.Select(index => Depots[index]).ToList();
        }

        /// <summary>
        /// Znajduje optymalny depot dla podanego klienta
        /// </summary>
        /// <param name="clientId">ID klienta</param>
        /// <param name="departure">Czas wyjazdu</param>
        /// <param name="beforeClient">trye - depot poprzedza klienta, false - depot następuje po kliencie</param>
        /// <returns>Optymalny depot</returns>
        public Depot GetClosestValidDepot(uint clientId, TimeSpan departure, bool beforeClient = false)
        {
            var depotList = GetDepotList(clientId);
            var client = GetClient(clientId);
            return !beforeClient
                ? (from depot in depotList
                    let arrival = departure + TimeSpan.FromMinutes(client.Location | depot.Location)
                    where arrival >= client.RealStartTime && arrival <= client.EndTime
                    select depot).FirstOrDefault()
                : (from depot in depotList
                let arrival = departure + TimeSpan.FromMinutes(client.Location | depot.Location)
                where arrival >= depot.StartTime && arrival <= depot.EndTime
                select depot).FirstOrDefault();
        }

        /// <summary>
        /// Znajduje optymalny depot początkowy względem pierwszego w kolejności klienta
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public Depot GetClosestValidStartingDepotToClient(uint clientId)
        {
            var depotList = GetDepotList(clientId);
            var client = GetClient(clientId);
            return (from depot in depotList
                    let departure = depot.StartTime
                    let arrival = departure + TimeSpan.FromMinutes(client.Location | depot.Location)
                    where arrival >= client.RealStartTime && arrival <= client.EndTime
                    select depot).FirstOrDefault();
        }

        /// <summary>
        /// Znajduje optymalny depot będący na drodze między dwoma klientami
        /// </summary>
        /// <param name="clientId1">ID pierwszego klienta</param>
        /// <param name="clientId2">ID drugiego klienta</param>
        /// <param name="departure">Czas wyjazdu od pierwszego klienta</param>
        /// <returns>Optymalny depot pomiędzy podanymi klientami</returns>
        public Depot GetClosestValidDepot(uint clientId1, uint clientId2, TimeSpan departure)
        {
            if (clientId1 == clientId2)
                throw new ArgumentException("Try to use GetClosestValidDepot(Client, Timespan, bool)");
            var depotList = GetDepotList(clientId1, clientId2);
            var startClient = GetClient(clientId1);
            var endclient = GetClient(clientId2);
            //foreach (var depot in depotList)
            //{
            //    var arrival1 = departure + TimeSpan.FromMinutes(startClient.Location | depot.Location);
            //    var arrival2 = arrival1 + TimeSpan.FromMinutes(depot.Location | endclient.Location);
            //    if (arrival1 >= depot.StartTime && arrival1 <= depot.EndTime &&
            //        arrival2 >= endclient.StartTime && arrival2 <= endclient.EndTime)
            //        return depot;
            //}
            //return null;
            //Poniższe robi to co powyższe:
            return (from depot in depotList
                    let arrival1 = departure + TimeSpan.FromMinutes(startClient.Location | depot.Location)
                    let arrival2 = arrival1 + TimeSpan.FromMinutes(depot.Location | endclient.Location)
                    where arrival1 >= depot.StartTime && arrival1 <= depot.EndTime && arrival2 >= endclient.RealStartTime && arrival2 <= endclient.EndTime
                    select depot).FirstOrDefault();
        }

        /// <summary>
        /// Znajduje pozycję obiektu (klienta lub depotu) o podanym ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
            foreach (var client in Clients.Where(client => client.Id == id))
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

            Dictionary<uint, uint> depotLocations = new Dictionary<uint, uint>();
            Dictionary<uint, uint> clientsLocations = new Dictionary<uint, uint>();

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
                    case "DEPOT_LOCATION_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            depotLocations.Add(Convert.ToUInt32(line[0]), Convert.ToUInt32(line[1]));
                        } while (!problemFileLines[i + 1].Any(Char.IsLetter));
                        break;
                    case "VISIT_LOCATION_SECTION":
                        do
                        {
                            i++;
                            string[] line = problemFileLines[i].Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
                            clientsLocations.Add(Convert.ToUInt32(line[0]), Convert.ToUInt32(line[1]));
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
                depots.Add(new Depot(locations[depotLocations[depotsIds[i]]], depotTimesWindows[depotsIds[i]].Item1, depotTimesWindows[depotsIds[i]].Item2, depotsIds[i]));
            }
            var maxEnd = depots.Max<Depot>((depot => (depot.EndTime.Ticks)));
         
            for (int i = 0; i < clientsIds.Count; i++)
            {
                clients.Add(new Client(locations[clientsLocations[clientsIds[i]]], timesAvivals[clientsIds[i]], new TimeSpan(maxEnd), unld[clientsIds[i]], demands[clientsIds[i]], clientsIds[i]));

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