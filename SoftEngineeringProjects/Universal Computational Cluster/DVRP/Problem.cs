using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using DVRP.Objects;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DVRP
{
    [Serializable]
    public class Problem
    {
        // vehicles
        private List<Vehicle> _fleet;
        // clients to handle
        private List<Client> _clients;
        // bases
        private List<Depot> _depots;

        // Pytanie - to mają być common ^, czy pchamy to wszyskim w ramach jednego problemu ->, a TN ma to puste?
        private int[][] _clientsOrder; /// Generowane dla kazdego podproblemu

        public Problem(List<Vehicle> fleet, List<Client> clients, List<Depot> depots, int[][] clientsOrder)
        {
            _fleet = fleet;
            _clients = clients;
            _depots = depots;
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
            try
            {
                IFormatter formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, this);
                byte[] byteArray = stream.ToArray();
                stream.Close();
                return byteArray;
            }
            catch (Exception e) { throw e; }
            return null;
        }

        public static Problem Deserialize(byte[] byteArray)
        {
            Problem problem = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream(byteArray);
                problem = (Problem)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e) { }
            return problem;
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