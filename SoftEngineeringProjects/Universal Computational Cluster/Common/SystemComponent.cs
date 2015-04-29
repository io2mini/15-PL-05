using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Common.Configuration;
using Common.Exceptions;
using Common.Messages;
using Common.Messages.Generators;
using Common.Properties;

namespace Common
{
    public enum SystemComponentType
    {
        CommunicationServer,
        BackupCommunicationServer,
        ComputationalClient,
        ComputationalNode,
        TaskManager
    }

    public abstract class SystemComponent
    {
        protected NoOperationBackupCommunicationServersBackupCommunicationServer BackupServer;
        protected CommunicationInfo communicationInfo;
        protected SystemComponentType DeviceType;
        protected byte PararellThreads;
        protected string[] SolvableProblems;
        protected TcpClient TcpClient;
        protected ThreadInfo ThreadInfo;

        protected SystemComponent()
        {
            IsWorking = true;
            /*
             * TODO: Initialize Thread Array
             */
            Initialize();
        }

        protected ulong Id { get; set; }
        public bool IsWorking { get; set; }

        public CommunicationInfo CommunicationInfo
        {
            get { return communicationInfo; }
            set { communicationInfo = value; }
        }

        /// <summary>
        ///     Metoda inicjalizujące słowniki do analizy wiadomości
        /// </summary>
        protected virtual void Initialize()
        {
            SchemaTypes = new Dictionary<string, Tuple<string, Type>>
            {
                {RegisterResponse, new Tuple<string, Type>(Resources.RegisterResponse, typeof (RegisterResponse))},
                {NoOperation, new Tuple<string, Type>(Resources.NoOperation, typeof (NoOperation))},
                {Error, new Tuple<string, Type>(Resources.Error, typeof (Error))},
                {Solution, new Tuple<string, Type>(Resources.Solution, typeof (Solutions))}
            };
        }

        /// <summary>
        ///     Metoda rozpoczynająca działanie komponentu (wysyła register message)
        ///     Metoda nadpisywana przez Communication Server
        /// </summary>
        public virtual void Start()
        {
            InitializeConnection();
            SendRegisterMessage();
            ReceiveResponse();
        }

        protected SystemComponentType ParseType(string s)
        {
            switch (s)
            {
                case "CommunicationServer":
                    return SystemComponentType.CommunicationServer;
                case "BackupCommunicationServer":
                    return SystemComponentType.BackupCommunicationServer;
                case "ComputationalClient":
                    return SystemComponentType.ComputationalClient;
                case "ComputationalNode":
                    return SystemComponentType.ComputationalNode;
                case "TaskManager":
                    return SystemComponentType.TaskManager;
                default:
                    throw new ArgumentException("Enum type not recognized");
            }
        }

        /// <summary>
        ///     Metoda mająca na celu wysłanie odpowiedniego komnuniktatu w zalezności od urządzenia,
        ///     na którym jest wywoływana.
        /// </summary>
        /// <param name="deviceType">Typ urządzenia rejestującego się.</param>
        private void SendRegisterMessage()
        {
            var msg = RegisterGenerator.Generate(DeviceType, SolvableProblems, PararellThreads, false, null);
            try
            {
                SendMessage(msg);
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine("Register Message Not Send");
            }
        }

        /// <summary>
        ///     Metoda wysyłajaca Error Message
        /// </summary>
        /// <param name="message">wiadomosć przekazywana w Error Message</param>
        protected void SendErrorMessage(string message, ErrorErrorType errorType)
        {
            SendMessage(ErrorGenerator.Generate(message, errorType));
        }

        /// <summary>
        ///     Metoda używana do otrzymywania wiadomści, wyświetla na konsolę otrzymany message
        /// </summary>
        protected void ReceiveResponse()
        {
            if (!TcpClient.Connected)
                TcpClient.Connect(communicationInfo.CommunicationServerAddress.Host,
                    communicationInfo.CommunicationServerPort);
            var stream = TcpClient.GetStream();
            var byteArray = new byte[1024];
            try
            {
                stream.Read(byteArray, 0, 1024);
            }
            catch (Exception)
            {
                Console.WriteLine("Connection was killed by host");
                return;
            }
            var message = Message.Sanitize(byteArray);
            //Console.WriteLine(message);
            Validate(message, null); //Uważać z nullem w klasach dziedziczących
        }

        #region Constants

        private const string RegisterResponse = "RegisterResponse",
            NoOperation = "NoOperation",
            Error = "Error",
            Solution = "Solution";

        private const uint MilisecondsMultiplier = 1000;
        public const string Path = ""; //Scieżka do pliku konfiguracyjnego

        #endregion

        #region MessageReactionFields

        /// <summary>
        ///     Słownik przyporzadkowujący nazwę komunikatu do pary odpowiadającej
        ///     treści XML Schema'y tego typu komunikatu oraz typowi obiektu odpowiadającego temu typowi komunikatu.
        /// </summary>
        protected Dictionary<string, Tuple<string, Type>> SchemaTypes;

        protected Timer StatusReporter;

        #endregion

        #region MessageQueueFields

        protected Queue<Tuple<string, Socket>> MessageQueue;
        protected AutoResetEvent MessageQueueMutex;

        #endregion

        #region MessageGenerationAndHandling

        /// <summary>
        ///     Ogólna metoda wywołująca odpowiedni handler dla otrzymanej wiadomości
        /// </summary>
        /// <param name="message">Otrzymana wiadomość</param>
        /// <param name="key">Nazwa schemy której otrzymujemy</param>
        /// <param name="socket">Socket z którego przyszła wiadomość</param>
        protected virtual void HandleMessage(Message message, string key, Socket socket)
        {
            switch (key)
            {
                case RegisterResponse:
                    MsgHandler_RegisterResponse((RegisterResponse) message);
                    return;
                case NoOperation:
                    MsgHandler_NoOperation((NoOperation) message);
                    return;
                case Error:
                    MsgHandler_Error((Error) message);
                    return;
                case Solution:
                    MsgHandler_Solution((Solutions) message);
                    return;
                default:
                    throw new InvalidOperationException("Unknown msg key"); //TODO: własny wyjątek;
            }
        }

        protected virtual void MsgHandler_Solution(Solutions solutions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Abstrakcyjna klasa do obsługi komunikatu o błędzie, obsługa zależna od odbiorcy.
        /// </summary>
        /// <param name="error"></param>
        protected virtual void MsgHandler_Error(Error message)
        {
            Console.WriteLine("Error Message");
            switch (message.ErrorType)
            {
                case ErrorErrorType.UnknownSender:
                {
                    StatusReporter.Dispose();
                    SendRegisterMessage();
                    break;
                }
                case ErrorErrorType.InvalidOperation:
                {
                    //TODO:switch to idle/partially idle state
                    throw new NotImplementedException();
                    break;
                }
            }
        }

        /// <summary>
        ///     Metoda reaguje na register response, tworząc  wątek, który regularnie co Timeout wysyła wiadomość
        ///     typu Status Report do serwera
        /// </summary>
        /// <param name="message"> Message typu Register Response na który reagujemy</param>
        protected void MsgHandler_RegisterResponse(RegisterResponse message)
        {
            Console.WriteLine("RegisterResponse Message id={0}", message.Id);
            communicationInfo.Time = message.Timeout;
            Id = message.Id;
            try
            {
                StatusReporter = new Timer(
                    o =>
                    {
                        Console.WriteLine("Sending Status");
                        SendMessage(GenerateStatus());
                        var thread = new Thread(ReceiveResponse);
                        thread.IsBackground = true;
                        thread.Start();
                    }, null, 0, (int) message.Timeout*MilisecondsMultiplier);
            }
            catch (InvalidIdException)
            {
                Console.WriteLine("Negative Id for component");
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine("Message Not send for component type {0} with id {1}", DeviceType, Id);
            }
        }

        protected virtual Status GenerateStatus()
        {
            return StatusReportGenerator.Generate(Id, ThreadInfo.Threads.ToArray());
        }

        /// <summary>
        ///     Metoda reaguje na NoOperation, aktualizując dane o Backup Serwerze
        /// </summary>
        /// <param name="message"> Message typu NoOperation na który reagujemy</param>
        protected void MsgHandler_NoOperation(NoOperation message)
        {
            Console.WriteLine("NoOperation Message");
            BackupServer = message.BackupCommunicationServers.BackupCommunicationServer;
        }

        /// <summary>
        ///     Metoda walidująca wiadomości z istniejącymi schemami
        /// </summary>
        /// <param name="XML">wiadomość w postaci łańcucha znaków</param>
        /// <param name="socket">Socket z którego otrzymano</param>
        protected void Validate(string XML, Socket socket)
        {
            XDocument message;
            try
            {
                message = XDocument.Parse(XML, LoadOptions.PreserveWhitespace);
            }
            catch (XmlException e)
            {
                //Console.WriteLine("Wrong msg\n" + e.Message + "\n" + XML + "\n");
                return;
            }
            foreach (var Key in SchemaTypes.Keys)
            {
                var schemas = new XmlSchemaSet();
                schemas.Add(null, XmlReader.Create(new StringReader(SchemaTypes[Key].Item1)));
                var errorOccured = false;
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
        ///     Metoda serializująca informacje komunikacyjne do pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        public virtual void SaveConfig(string path)
        {
            var xmlSerializer = new XmlSerializer(typeof (CommunicationInfo));
            xmlSerializer.Serialize(new FileStream(path, FileMode.Create), communicationInfo);
        }

        /// <summary>
        ///     Metoda deserializująca informacje komunikacyjne z pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        public virtual void LoadConfig(string path)
        {
            var xmlDeSerializer = new XmlSerializer(typeof (CommunicationInfo));
            try
            {
                communicationInfo = (CommunicationInfo) xmlDeSerializer.Deserialize(new FileStream(path, FileMode.Open));
            }
            catch (FileNotFoundException e)
            {
                throw new ArgumentException("Config file not found", e);
            }
        }

        #endregion

        #region ConnectionHandling

        /// <summary>
        ///     Metoda inicjalizująca połączenie do serwera
        /// </summary>
        protected void InitializeConnection()
        {
            try
            {
                TcpClient = new TcpClient(communicationInfo.CommunicationServerAddress.Host,
                    communicationInfo.CommunicationServerPort);
            }
            catch (SocketException e)
            {
                var message = string.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    communicationInfo.CommunicationServerAddress.Host, communicationInfo.CommunicationServerPort);
                throw new ConnectionException(message, e);
            }
        }

        /// <summary>
        ///     Metoda wysyłająca Message do serwera
        /// </summary>
        /// <param name="m">Message do wysłania</param>
        protected void SendMessage(Message m)
        {
            try
            {
                var message = m.toString();
                //Console.WriteLine(message);
                if (!TcpClient.Connected)
                {
                    TcpClient.Connect(communicationInfo.CommunicationServerAddress.Host,
                        communicationInfo.CommunicationServerPort);
                }
                var stream = TcpClient.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) {AutoFlush = false};
                writer.Write(message);
                writer.Flush();
            }
            catch (Exception e)
            {
                var message = "Unable to send message";
                throw new MessageNotSentException(message, e);
            }
        }

        #endregion
    }
}