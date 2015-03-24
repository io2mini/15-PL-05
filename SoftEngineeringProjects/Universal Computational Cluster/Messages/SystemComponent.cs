using Common.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common
{
    public abstract class SystemComponent
    {
        protected CommunicationInfo communicationServerInfo;
        public CommunicationInfo CommunicationServerInfo { get { return communicationServerInfo; } set { communicationServerInfo = value; } }
        protected virtual void SaveConfig(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CommunicationInfo));
            xmlSerializer.Serialize(new FileStream(path, FileMode.Create), communicationServerInfo);

        }
        protected virtual void LoadConfig(string path)
        {
            XmlSerializer xmlDeSerializer = new XmlSerializer(typeof(CommunicationInfo));
            try
            {
                communicationServerInfo = (CommunicationInfo)xmlDeSerializer.Deserialize(new FileStream(path, FileMode.Open));
            }
            catch (FileNotFoundException e)
            {
                throw new ArgumentException("Config file not found", e);
            }
        }
    }
}
