using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Client
    {
        private TimeSpan _endTime;
        private Location _location;
        // size of transport
        private double _size;
        private TimeSpan _startTime;
        //max time needed for transport
        private double _unld;

        public Client(Location location, TimeSpan startTime, TimeSpan endTime, double unld, double size, uint id)
        {
            _location = location;
            _startTime = startTime;
            _endTime = endTime;
            _unld = unld;
            _size = size;
            Id = id;
        }

        public uint Id { get; private set; }
    }
}