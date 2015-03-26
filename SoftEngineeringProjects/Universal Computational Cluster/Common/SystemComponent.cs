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
using System.Net;

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
        #region Constants
        const String RegisterResponse = "RegisterResponse", NoOperation = "NoOperation", Error = "Error";
        const uint MilisecondsMultiplier = 1000;
        public const string Path = ""; //?????????????
        #endregion
        protected CommunicationInfo communicationInfo;
        protected NoOperationBackupCommunicationServersBackupCommunicationServer BackupServer;
        protected TcpClient tcpClient;
        #region MessageReactionFields
        protected Dictionary<string, string> Schemas;
        protected Dictionary<string, Type> MessageTypes;
        protected List<string> DictionaryKeys;
        protected Timer StatusReporter;
        #endregion
        #region MessageQueueFields
        protected Queue<Tuple<String, Socket>> MessageQueue;
        protected AutoResetEvent MessageQueueMutex;
        #endregion

        protected ulong Id { get; set; }

        public bool IsWorking { get; set; }

        public SystemComponent()
        {
            IsWorking = true;
            Initialize();
        }

        /// <summary>
        /// Metoda inicjalizujące słowniki do analizy wiadomości
        /// </summary>
        protected virtual void Initialize()
        {
            //Inicjalizacja
            DictionaryKeys = new List<string>();
            Schemas = new Dictionary<string, string>();
            MessageTypes = new Dictionary<string, Type>();
            //RegsterResponse
            DictionaryKeys.Add(RegisterResponse);
            Schemas.Add(RegisterResponse, Resources.RegisterResponse);
            MessageTypes.Add(RegisterResponse, typeof(RegisterResponse));
            //NoOperation
            DictionaryKeys.Add(NoOperation);
            Schemas.Add(NoOperation, Resources.NoOperation);
            MessageTypes.Add(NoOperation, typeof(NoOperation));
            //Error
            DictionaryKeys.Add(Error);
            Schemas.Add(Error, Resources.Error);
            MessageTypes.Add(NoOperation, typeof(Error));
        }

        /// <summary>
        /// Metoda rozpoczynająca działanie komponentu (wysyła register message)
        /// Metoda nadpisywana przez Communication Server
        /// </summary>
        public virtual void Start()
        {
            InitializeMessageQueue();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Metoda inicializująca kolejkę wiadomości
        /// </summary>
        protected void InitializeMessageQueue()
        {
            MessageQueueMutex = new AutoResetEvent(true);
            MessageQueue = new Queue<Tuple<String, Socket>>();
            Thread thread = new Thread(MessageQueueWork);
            thread.IsBackground = true;
            thread.Start();
            Thread listener = new Thread(StartListening);
            listener.IsBackground = true;
            listener.Start();
        }

        /// <summary>
        /// Metoda obsługi kolejki wiadomości
        /// </summary>
        private void MessageQueueWork()
        {
            while(true)
            {
                MessageQueueMutex.WaitOne();
                while(MessageQueue.Count > 0)
                {
                    var Message = MessageQueue.Dequeue();
                    Validate(Message.Item1, Message.Item2);
                }
            }
        }

        /// <summary>
        /// Metoda nawiązująca połączenie z nadającym wiadomości komponentem.
        /// </summary>
        public void StartListening()
        {
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse(communicationInfo.CommunicationServerAddress.AbsolutePath);
            TcpListener tcpListener = new TcpListener(ipAddress, (int)communicationInfo.CommunicationServerPort);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, (int)communicationInfo.CommunicationServerPort);
            try
            {
                while (IsWorking)
                {
                    Socket socket = tcpListener.AcceptSocket();
                    Thread thread = new Thread(new ParameterizedThreadStart(ReceiveMessage));
                    thread.IsBackground = true;
                    thread.Start(socket);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Metoda pośrednia do obsługi nasłuchu.
        /// </summary>
        /// <param name="socket">sochet, na którym nasłuchujemy.</param>
        private void ReceiveMessage(Object socket)
        {
            ReceiveMessage((Socket)socket);
        }

        /// <summary>
        /// Metoda obsługująca nasłuchiwanie komunikatów od konkretnego komponentu.
        /// </summary>
        /// <param name="socket">Socket na którym odbywa się nasłuchiwanie.</param>
        private void ReceiveMessage(Socket socket)
        {
            while (socket.IsBound)
            {
                byte[] byteArray = new Byte[1024];
                //TODO: try catch?
                socket.Receive(byteArray);
                String message = System.Text.Encoding.UTF8.GetString(byteArray);
                MessageQueue.Enqueue(new Tuple<string, Socket>(message, socket));
                MessageQueueMutex.Reset();
            }
        }

        /// <summary>
        /// ???????????????????????
        /// </summary>
        protected void HandShake()
        {

        }

        public CommunicationInfo CommunicationInfo
        {
            get { return communicationInfo; }
            set { communicationInfo = value; }
        }

        #region MessageGenerationAndHandling
        /// <summary>
        /// Abstrakcyjna metoda generująca Status Report komponentu w zależności od komponentu
        /// </summary>
        /// <returns>Status - Status Report komponentu</returns>
        protected abstract Status GenerateStatusReport();

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
                    MsgHandler_RegisterResponse((RegisterResponse)message);
                    return;
                case NoOperation:
                    MsgHandler_NoOperation((NoOperation)message);
                    return;
                case Error:
                    MsgHandler_Error((Error)message);
                    return;
            }
        }

        /// <summary>
        /// Abstrakcyjna klasa do obsługi komunikatu o błędzie, obsługa zależna od odbiorcy.
        /// </summary>
        /// <param name="error"></param>
        protected abstract void MsgHandler_Error(Messages.Error message);

        /// <summary>
        /// Metoda reaguje na register response, tworząc  wątek, który regularnie co Timeout wysyła wiadomość
        /// typu Status Report do serwera
        /// </summary>
        /// <param name="message"> Message typu Register Response na który reagujemy</param>
        protected virtual void MsgHandler_RegisterResponse(RegisterResponse message)
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
        protected virtual void MsgHandler_NoOperation(NoOperation message)
        {
            BackupServer = message.BackupCommunicationServers.BackupCommunicationServer;
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
                schemas.Add(null, XmlReader.Create(new StringReader(Schemas[Key])));
                bool errorOccured = false;
                message.Validate(schemas, (o, e) => { errorOccured = true; });
                if (!errorOccured)
                {
                    HandleMessage(Message.ParseXML(MessageTypes[Key], XML), Key, socket);
                }
            }
        }
        #endregion

        #region ConfigFilesHandling
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
        #endregion

        #region ConnectionHandling
        /// <summary>
        /// Metoda inicjalizująca połączenie do serwera
        /// </summary>
        protected void InitializeConnection()
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
        #endregion
    }
}
