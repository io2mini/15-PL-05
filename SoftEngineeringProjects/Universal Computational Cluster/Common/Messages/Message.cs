using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common
{
    [System.SerializableAttribute()]
    public abstract class Message
    {
        static public Message ParseXML(Type type)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
        }
        public String GetMessage()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}
