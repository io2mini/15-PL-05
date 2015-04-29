using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Vehicle
    {
        private int _capacity;
        private Location _location;
        private int _speed;

        public Vehicle(Location location, int capacity, int speed)
        {
            _location = location;
            _capacity = capacity;
            _speed = speed;
        }
    }
}