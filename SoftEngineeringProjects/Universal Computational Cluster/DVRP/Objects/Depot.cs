using System;
using System.Collections.Generic;
using System.Drawing;

namespace DVRP.Objects
{
    [Serializable]
    public class Depot
    {
        public uint Id { get; private set; }
        public Location Location { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public Depot (Location location, TimeSpan startTime, TimeSpan endTime, uint id)
        {
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            Id = id;
        }
    }
}