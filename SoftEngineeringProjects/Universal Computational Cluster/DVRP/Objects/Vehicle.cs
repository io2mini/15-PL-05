using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP.Objects
{
    [Serializable]
    public class Vehicle
    {
        public Location Location { get; private set; }
        public int Capacity { get; private set; }
        public int Speed { get; private set; }

        public Vehicle(Location location, int capacity, int speed)
        {
            Location = location;
            Capacity = capacity;
            Speed = speed;
        }
    }
}
