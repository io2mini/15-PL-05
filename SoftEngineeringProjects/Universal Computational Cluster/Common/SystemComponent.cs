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
using Common.Messages.Generators;
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
        public const string Path = ""; //Scieżka do pliku konfiguracyjnego
        #endregion
        protected CommunicationInfo communicationInfo;
        protected NoOperationBackupCommunicationServersBackupCommunicationServer BackupServer;
        protected TcpClient tcpClient;
        #region MessageReactionFields
        /// <summary>
        /// Słownik przyporzadkowujący nazwę komunikatu do pary odpowiadającej
        /// treści XML Schema'y tego typu komunikatu oraz typowi obiektu odpowiadającego temu typowi komunikatu.
        /// </summary>
        protected Dictionary<string, Tuple<string, Type>> SchemaTypes;
        protected Timer StatusReporter;
        #endregion
        #region MessageQueueFields
        protected Queue<Tuple<String, Socket>> MessageQueue;
        protected AutoResetEvent MessageQueueMutex;
        #endregion

        protected SystemComponentType deviceType;
        protected string[] solvableProblems;
        protected byte pararellThreads;

        protected ulong Id { get; set; }

        public bool IsWorking { get; set; }

        public CommunicationInfo CommunicationInfo
        {
            get { return communicationInfo; }
            set { communicationInfo = value; }
        }

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
            SchemaTypes = new Dictionary<string, Tuple<string, Type>>();
            SchemaTypes.Add(RegisterResponse,new Tuple<string, Type>(Resources.RegisterResponse, typeof(RegisterResponse)));
            SchemaTypes.Add(NoOperation, new Tuple<string, Type>(Resources.NoOperation, typeof(NoOperation)));
            SchemaTypes.Add(Error, new Tuple<string, Type>(Resources.Error, typeof(Error)));
        }

        /// <summary>
        /// Metoda rozpoczynająca działanie komponentu (wysyła register message)
        /// Metoda nadpisywana przez Communication Server
        /// </summary>
        public virtual void Start()
        {
            Random random = new Random();
            while (true)
            {
                try
                {
                    InitializeMessageQueue(random.Next(100, 10000));
                    break;
                }
                catch (SocketException e)
                {
                    continue;
                }
            }

            InitializeConnection();
            SendRegisterMessage();
            ReceiveResponse();
        }

        /// <summary>
        /// Metoda mająca na celu wysłanie odpowiedniego komnuniktatu w zalezności od urządzenia,
        /// na którym jest wywoływana.
        /// </summary>
        /// <param name="deviceType">Typ urządzenia rejestującego się.</param>
        private void SendRegisterMessage()
        {
            Register msg = RegisterGenerator.Generate(deviceType, solvableProblems, pararellThreads, false, null);
            SendMessage(msg);
            
        }

        /// <summary>
        /// Metoda inicializująca kolejkę wiadomości do odbioru
        /// </summary>
        protected void InitializeMessageQueue(int port)
        {
            MessageQueueMutex = new AutoResetEvent(true);
            MessageQueue = new Queue<Tuple<String, Socket>>();
            Thread thread = new Thread(MessageQueueWork);
            thread.IsBackground = true;
            thread.Start();
            Thread listener = new Thread(new ParameterizedThreadStart(StartListening));
            listener.IsBackground = true;
            listener.Start((object)port);
        }

        /// <summary>
        /// Metoda obsługi kolejki wiadomości odbierającej komunikaty
        /// </summary>
        private void MessageQueueWork()
        {
            while(true)
            {
                MessageQueueMutex.WaitOne();
                MessageQueueMutex.Reset();
                while(MessageQueue.Count > 0)
                {
                    var Message = MessageQueue.Dequeue();
                    Console.WriteLine(Message.Item1);
                    Validate(Message.Item1, Message.Item2);
                }
            }
        }
        //TODO: Move to message
        /// <summary>
        /// Converts byteArray to string and removes unnecessary characters.
        /// </summary>
        /// <param name="byteArray">Message in byte form</param>
        /// <returns>Message in string form</returns>
        public string Sanitize(byte[] byteArray)
        {
          var message=  System.Text.Encoding.UTF8.GetString(byteArray);
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (message.StartsWith(_byteOrderMarkUtf8))
            {
                message = message.Replace(_byteOrderMarkUtf8, "");
            }
            message = message.Replace("\0", string.Empty);
            return message;
        }
        public void ReceiveResponse()
        {
           var stream= tcpClient.GetStream();
           byte[] byteArray = new byte[1024];
            stream.Read(byteArray, 0, 1024);
            String message = Sanitize(byteArray);
            Console.WriteLine(message);
            Validate(message, null); //Uważać z nullem w klasach dziedziczących
        }
        /// <summary>
        /// Metoda nawiązująca połączenie z nadającym wiadomości komponentem.
        /// </summary>
        public void StartListening(object Port)
        {
            int port = (int)Port;
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse(communicationInfo.CommunicationServerAddress.Host);
            TcpListener tcpListener = new TcpListener(ipAddress, port);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            try
            {
                tcpListener.Start();
                while (IsWorking)
                {
                    Console.WriteLine("Po dekalracji");
                    Socket socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Po accept");
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
            try
            {
                while (socket.IsBound)
                {
                    byte[] byteArray = new Byte[1024];
                    //TODO: try catch?
                    Thread.Sleep(1000);
                    int offset = 0;
                    int readamount;
                    socket.Receive(byteArray);
                    
                      
                    String message = Sanitize(byteArray);
                    MessageQueue.Enqueue(new Tuple<string, Socket>(message, socket));
                    MessageQueueMutex.Set();

                }
            }
            catch (SocketException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        #region MessageGenerationAndHandling
        /// <summary>
        /// Abstrakcyjna metoda generująca Status Report komponentu w zależności od komponentu
        /// </summary>
        /// <returns>Status - Status Report komponentu</returns>
        protected virtual Status GenerateStatusReport()
        {
            if (this.Id < 0) throw new InvalidOperationException(); //TODO: stworzyć własny wyjątek
            var result = new Status();
            result.Id = this.Id;
            return result;
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
        protected virtual void MsgHandler_Error(Messages.Error message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Metoda reaguje na register response, tworząc  wątek, który regularnie co Timeout wysyła wiadomość
        /// typu Status Report do serwera
        /// </summary>
        /// <param name="message"> Message typu Register Response na który reagujemy</param>
        protected virtual void MsgHandler_RegisterResponse(RegisterResponse message)
        {
            communicationInfo.Time = message.Timeout;
            Id = message.Id;
            StatusReporter = new Timer((o) => { SendMessage(GenerateStatusReport()); ReceiveResponse(); }, null, 0,
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
            XDocument message = XDocument.Parse(XML,LoadOptions.PreserveWhitespace);
            foreach (String Key in this.SchemaTypes.Keys)
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(null, XmlReader.Create(new StringReader(SchemaTypes[Key].Item1)));
                bool errorOccured = false;
                message.Validate(schemas, (o, e) => { errorOccured = true; });
                if (!errorOccured)
                {
                    HandleMessage(Message.ParseXML(SchemaTypes[Key].Item2, XML), Key, socket);
                    break;
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
                tcpClient = new TcpClient(communicationInfo.CommunicationServerAddress.Host,
                (int)communicationInfo.CommunicationServerPort );
               
            }
            catch (SocketException e)
            {
                String message = String.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    communicationInfo.CommunicationServerAddress.Host, communicationInfo.CommunicationServerPort);
                throw new ConnectionException(message, e);
            }
            
        }

        /// <summary>
        /// Metoda wysyłająca Message do serwera
        /// </summary>
        /// <param name="m">Message do wysłania</param>
        protected void SendMessage(Message m)
        {
            try
            {
                String message = m.toString();
                Console.WriteLine(message);
                NetworkStream stream = tcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = false;
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
