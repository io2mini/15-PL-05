using System;

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
