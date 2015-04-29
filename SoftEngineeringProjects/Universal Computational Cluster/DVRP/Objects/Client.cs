using System;
using System.Drawing;

namespace DVRP.Objects
{
    [Serializable]
    public class Client
    {
        private uint _id;
        private Location _location;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        //max time needed for transport
        private double _unld;
        // size of transport
        private double _size;

        public Client (Location location, TimeSpan startTime, TimeSpan endTime, double unld, double size, uint id )
        {
            _location = location;
            _startTime = startTime;
            _endTime = endTime;
            _unld = unld;
            _size = size;
            _id = id;
        }

        public uint Id
        {
            get { return _id; }
        }
    }
}