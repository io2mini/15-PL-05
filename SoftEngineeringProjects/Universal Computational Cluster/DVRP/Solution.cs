using System;
using System.Collections.Generic;
using System.Linq;
using DVRP.Objects;

namespace DVRP
{
    [Serializable]
    public class Solution
    {
        // cost of 
        private readonly List<Route> _vehicleLocationList;

        public Solution(List<Route> vehicleLocationList)
        {
            if (_vehicleLocationList == null) throw new ArgumentNullException();
            _vehicleLocationList = vehicleLocationList;
        }

        public double Cost { get; private set; }

        //Czy z tego korzystamy?
        //public void CountCost(List<Location> locations)
        //{
        //    Cost = 0;
        //    for (var i = 0; i < _vehicleLocationList.Count(); i++)
        //    {
        //        if (_vehicleLocationList[i] == null) throw new NullReferenceException();
        //        for (var j = 1; j < _vehicleLocationList[i].Count(); j++)
        //        {
        //            Cost += locations[_vehicleLocationList[i][j]] | locations[_vehicleLocationList[i][j]];
        //        }
        //    }
        //}

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }

        public static Solution Deserialize(byte[] byteArray)
        {
            return (Solution) Serializer.Deserialize(byteArray);
        }
    }
}