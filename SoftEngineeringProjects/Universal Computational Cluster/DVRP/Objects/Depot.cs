using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Depot
    {
        private TimeSpan _endTime;
        private Location _location;
        private TimeSpan _startTime;

        public Depot(Location location, TimeSpan startTime, TimeSpan endTime, uint id)
        {
            _location = location;
            _startTime = startTime;
            _endTime = endTime;
            Id = id;
        }

        public uint Id { get; private set; }
    }
}