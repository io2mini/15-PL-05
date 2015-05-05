using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP.Objects
{
    [Serializable]
    public class Route
    {
        public uint[] Sequence { get; private set; }

        public Route(uint[] sequence)
        {
            Sequence = sequence;
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
