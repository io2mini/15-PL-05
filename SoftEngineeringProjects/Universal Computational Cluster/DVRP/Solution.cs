using DVRP.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    [Serializable]
    public class Solution
    {
        // cost of 
        private double _cost;
        private List<List<int>> _vehicleLocationList;

        public double Cost
        {
            get
            {
                return _cost;
            }

        }

        public Solution(List<List<int>> vehicleLocationList)
        {
            if (_vehicleLocationList == null) throw new ArgumentNullException();
            _vehicleLocationList = vehicleLocationList;
        }

        public void CountCost(List<Location> locations)
        {
            _cost = 0;
            for (int i = 0; i < _vehicleLocationList.Count(); i++)
            {
                if (_vehicleLocationList[i] == null) throw new NullReferenceException();
                for (int j = 1; j < _vehicleLocationList[i].Count(); j++)
                {
                    _cost += locations[_vehicleLocationList[i][j]] | locations[_vehicleLocationList[i][j]];
                }
            }
        }

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }

        public static Solution Deserialize(byte[] byteArray)
        {
            return (Solution)Serializer.Deserialize(byteArray);
        }
    }
}
