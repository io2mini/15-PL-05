using System;

namespace DVRP.Objects
{
    [Serializable]
    public class Route
    {
        public uint[] Sequence { get; private set; }
        public bool[] DepotAfter { get; set; }

        public Route(uint[] sequence)
        {
            Sequence = sequence;
        }

        public Route(uint[] sequence, bool[] depotAfter)
            : this(sequence)
        {
            DepotAfter = depotAfter;
        }


        public static Route Deserialize(byte[] byteArray)
        {
            return (Route)Serializer.Deserialize(byteArray);
        }

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }
    }
}
