using Common.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.Exceptions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;

namespace Common
{
    public abstract class SystemComponent
    {
        protected CommunicationInfo communicationServerInfo;
        protected TcpClient tcpClient;
        protected Dictionary<string, string> Schemas;
        protected Dictionary<string, Type> MessageTypes;
        protected List<string> DictionaryKeys;
        public CommunicationInfo CommunicationServerInfo
        { 
            get { return communicationServerInfo; } 
            set { communicationServerInfo = value; } 
        }
        protected virtual void HandleMessage(Message message, string key)
        {
            switch (key)
            {
                
            }
        }
        protected virtual void Validate(string XML)
        {
            XDocument Message = XDocument.Parse(XML);
            foreach (String Key in DictionaryKeys)
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", XmlReader.Create(new StringReader(Schemas[Key])));
                bool error = false;
                Message.Validate(schemas, (o, e) => { error = true; });
                if(!error)
                {
                    HandleMessage(
                }

            }
        }
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

        protected void InicializeConnection()
        {
            try{
                tcpClient = new TcpClient(communicationServerInfo.CommunicationServerAddress.AbsolutePath, 
                (int) communicationServerInfo.CommunicationServerPort);
            }
            catch(SocketException e)
            {
                String message = String.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    communicationServerInfo.CommunicationServerAddress.AbsolutePath, communicationServerInfo.CommunicationServerPort);
                throw new ConnectionException(message,e);
            }
            
        }

        protected void SendMessage(Message m)
        {
            try
            {
                String message = m.GetMessage();
                NetworkStream stream = tcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = false;
                writer.Write(Encoding.UTF8.GetBytes(message).Length);
                writer.Write(message);
                writer.Flush();
            }
            catch(Exception e)
            {
                String message = "Unable to send message";
                throw new MessageNotSentException(message, e);
            }
        }
    }
}
