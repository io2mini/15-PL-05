using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Client
    {
        public uint Id { get; private set; }
        public Location Location { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan RealStartTime
        {
            get
            {
                return CutOff ? new TimeSpan() : StartTime;
            }
        }
        public Boolean CutOff {get;set;} 
        
        public TimeSpan EndTime { get; private set; }
        //max time needed for transport
        public double Unld { get; private set; }
        // size of transport
        public double Size { get; private set; }

        public Client (Location location, TimeSpan startTime, TimeSpan endTime, double unld, double size, uint id,bool cutoff = false )
        {
            Location = location;
            StartTime = startTime;
            EndTime = endTime;
            Unld = unld;
            Size = size;
            Id = id;
            CutOff = cutoff;
        }
    }
}