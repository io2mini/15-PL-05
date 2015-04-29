using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DVRP
{
    public static class Serializer
    {
        public static byte[] Serialize(object o)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, o);
                var byteArray = stream.ToArray();
                stream.Close();
                return byteArray;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        public static object Deserialize(byte[] byteArray)
        {
            object o = null;
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream(byteArray);
                o = formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
            }
            return o;
        }
    }
}