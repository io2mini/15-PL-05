using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common.Messages
{
    [System.SerializableAttribute()]
    public abstract class Message
    {
        static public Message ParseXML(Type type,string Message)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            Message message = (Message)xmlSerializer.Deserialize(new StringReader(Message));
            return message;
        }
        public String toString()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}
