using System;
using System.Collections.Generic;
using System.Drawing;

namespace DVRP.Objects
{
    [Serializable]
    public class Depot
    {
        private Tuple<double, double> _location;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public Depot (Tuple<double, double> location, TimeSpan startTime, TimeSpan endTime)
        {
            _location = location;
            _startTime = startTime;
            _endTime = endTime;
        }
    }
}