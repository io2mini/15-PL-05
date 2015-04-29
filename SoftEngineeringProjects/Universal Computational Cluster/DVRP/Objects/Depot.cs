using System;
using System.Collections.Generic;
using System.Drawing;

namespace DVRP.Objects
{
    [Serializable]
    public class Depot
    {
        private uint _id;
        private Location _location;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public Depot (Location location, TimeSpan startTime, TimeSpan endTime, uint id)
        {
            _location = location;
            _startTime = startTime;
            _endTime = endTime;
            _id = id;
        }

        public uint Id
        {
            get { return _id; }
        }
    }
}