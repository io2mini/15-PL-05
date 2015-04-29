using System;
using System.Collections.Generic;
using System.Linq;
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

        public Solution(List<List<int>> vehicleLocationList)
        {
            _vehicleLocationList = vehicleLocationList;
        }

        public void CountCost(List<Object> locations)
        {

        }
    }
}
