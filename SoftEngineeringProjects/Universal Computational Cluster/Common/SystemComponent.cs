using Common.Communication;
using Common.Exceptions;
using Common.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using Common.Properties;
using System.Threading;

namespace Common
{
    public enum SystemComponentType
    {
        CommunicationServer,
        BackupCommunicationServer,
        ComputationalClient,
        ComputationalNode,
        TaskManager,
    }

    public abstract class SystemComponent
    {

        protected CommunicationInfo communicationInfo;
        protected NoOperationBackupCommunicationServersBackupCommunicationServer BackupServer;
        protected TcpClient tcpClient;
        protected Dictionary<string, string> Schemas;
        protected Dictionary<string, Type> MessageTypes;
        protected List<string> DictionaryKeys;
        protected ulong Id { get; set; }
        const String RegisterResponse = "RegisterResponse", NoOperation = "NoOperation";
        const uint MilisecondsMultiplier = 1000;
        public const string Path = "";
        public bool IsWorking { get; set; }
        protected Timer StatusReporter;

        /// <summary>
        /// Metoda rozpoczynająca działanie komponentu (wysyła register message)
        /// </summary>
        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public SystemComponent()
        {
            IsWorking = true;
            Initialize();
        }

        public CommunicationInfo CommunicationInfo
        {
            get { return communicationInfo; }
            set { communicationInfo = value; }
        }

        /// <summary>
        /// Metoda generująca Status Report komponentu do wysłania do serwera
        /// </summary>
        /// <returns>Status - Status Report komponentu</returns>
        protected Status GenerateStatusReport()
        {
            //TODO: Faktycznie wypełnić raport
            return new Status();
        }

        /// <summary>
        /// Metoda reaguje na register response, tworząc  wątek, który regularnie co Timeout wysyła wiadomość
        /// typu Status Report do serwera
        /// </summary>
        /// <param name="message"> Message typu Register Response na który reagujemy</param>
        protected virtual void RegisterResponseHandler(RegisterResponse message)
        {
            communicationInfo.Time = message.Timeout;
            Id = message.Id;
            StatusReporter = new Timer((o) => { SendMessage(GenerateStatusReport()); }, null, 0, 
                (int)message.Timeout * MilisecondsMultiplier);
        }

        /// <summary>
        /// Metoda reaguje na NoOperation, aktualizując dane o Backup Serwerze
        /// </summary>
        /// <param name="message"> Message typu NoOperation na który reagujemy</param>
        protected virtual void NoOperationHandler(NoOperation message)
        {
            BackupServer = message.BackupCommunicationServers.BackupCommunicationServer;
        }

        /// <summary>
        /// Ogólna metoda wywołująca odpowiedni handler dla otrzymanej wiadomości
        /// </summary>
        /// <param name="message">Otrzymana wiadomość</param>
        /// <param name="key">Nazwa schemy której otrzymujemy</param>
        /// <param name="socket">Socket z którego przyszła wiadomość</param>
        protected virtual void HandleMessage(Message message, string key, Socket socket)
        {
            switch (key)
            {
                case RegisterResponse:
                    RegisterResponseHandler((RegisterResponse)message);
                    return;
                case NoOperation:
                    NoOperationHandler((NoOperation)message);
                    return;
            }
        }

        /// <summary>
        /// Metoda inicjalizujące słowniki do analizy wiadomości
        /// </summary>
        protected virtual void Initialize()
        {
            DictionaryKeys = new List<string>();
            Schemas = new Dictionary<string, string>();
            MessageTypes = new Dictionary<string, Type>();
            DictionaryKeys.Add(RegisterResponse);
            DictionaryKeys.Add(NoOperation);
            Schemas.Add(RegisterResponse, Resources.RegisterResponse);
            Schemas.Add(NoOperation, Resources.NoOperation);
            MessageTypes.Add(RegisterResponse, typeof(RegisterResponse));
            MessageTypes.Add(NoOperation, typeof(NoOperation));
        }

        /// <summary>
        /// Metoda walidująca wiadomości z istniejącymi schemami
        /// </summary>
        /// <param name="XML">wiadomość w postaci łańcucha znaków</param>
        /// <param name="socket">Socket z którego otrzymano</param>
        protected virtual void Validate(string XML, Socket socket)
        {
            XDocument message = XDocument.Parse(XML);
            foreach (String Key in DictionaryKeys)
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", XmlReader.Create(new StringReader(Schemas[Key])));
                bool error = false;
                message.Validate(schemas, (o, e) => { error = true; });
                if (!error)
                {
                    HandleMessage(Message.ParseXML(MessageTypes[Key], XML), Key, socket);
                }
            }
        }

        /// <summary>
        /// Metoda serializująca informacje komunikacyjne do pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        public virtual void SaveConfig(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CommunicationInfo));
            xmlSerializer.Serialize(new FileStream(path, FileMode.Create), communicationInfo);
        }

        /// <summary>
        /// Metoda deserializująca informacje komunikacyjne z pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        public virtual void LoadConfig(string path)
        {
            XmlSerializer xmlDeSerializer = new XmlSerializer(typeof(CommunicationInfo));
            try
            {
                communicationInfo = (CommunicationInfo)xmlDeSerializer.Deserialize(new FileStream(path, FileMode.Open));
            }
            catch (FileNotFoundException e)
            {
                throw new ArgumentException("Config file not found", e);
            }
        }

        /// <summary>
        /// Metoda inicjalizująca połączenie do serwera
        /// </summary>
        protected void InicializeConnection()
        {
            try
            {
                tcpClient = new TcpClient(communicationInfo.CommunicationServerAddress.AbsolutePath,
                (int)communicationInfo.CommunicationServerPort);
            }
            catch (SocketException e)
            {
                String message = String.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    communicationInfo.CommunicationServerAddress.AbsolutePath, communicationInfo.CommunicationServerPort);
                throw new ConnectionException(message, e);
            }

        }

        /// <summary>
        /// Metoda wysyłająca Message do serwera
        /// </summary>
        /// <param name="m">Message do wysłania</param>
        protected void SendMessage(Message m)
        {
            //TO DO BCS Handling
            try
            {
                String message = m.toString();
                NetworkStream stream = tcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = false;
                writer.Write(Encoding.UTF8.GetBytes(message).Length);
                writer.Write(message);
                writer.Flush();
            }
            catch (Exception e)
            {
                String message = "Unable to send message";
                throw new MessageNotSentException(message, e);
            }
        }
    }
}
