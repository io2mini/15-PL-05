using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    public static class Serializer
    {
        public static byte[] Serialize(Object o)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, o);
                byte[] byteArray = stream.ToArray();
                stream.Close();
                return byteArray;
            }
            catch (Exception e) { }
            return null;
        }

        public static Object Deserialize(byte[] byteArray)
        {
            Object o = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream(byteArray);
                o = formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e) { }
            return o;
        }
    }
}
