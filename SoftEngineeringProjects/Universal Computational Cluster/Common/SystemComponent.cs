using Common.Configuration;
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
        const String RegisterResponse = "RegisterResponse", NoOperation = "NoOperation", Error = "Error", Solution = "Solution";
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
        protected ThreadInfo threadInfo;
        public CommunicationInfo CommunicationInfo
        {
            get { return communicationInfo; }
            set { communicationInfo = value; }
        }

        public SystemComponent()
        {
            IsWorking = true;
            /*
             * TODO: Initialize Thread Array
             */
            Initialize();
        }

        /// <summary>
        /// Metoda inicjalizujące słowniki do analizy wiadomości
        /// </summary>
        protected virtual void Initialize()
        {
            SchemaTypes = new Dictionary<string, Tuple<string, Type>>();
            //RegisterResponse
            SchemaTypes.Add(RegisterResponse,new Tuple<string, Type>(Resources.RegisterResponse, typeof(RegisterResponse)));
            //NoOperation
            SchemaTypes.Add(NoOperation, new Tuple<string, Type>(Resources.NoOperation, typeof(NoOperation)));
            //Error
            SchemaTypes.Add(Error, new Tuple<string, Type>(Resources.Error, typeof(Error)));
            //Solution
            SchemaTypes.Add(Solution, new Tuple<string, Type>(Resources.Solution, typeof(Solutions)));
        }

        /// <summary>
        /// Metoda rozpoczynająca działanie komponentu (wysyła register message)
        /// Metoda nadpisywana przez Communication Server
        /// </summary>
        public virtual void Start()
        {
            InitializeConnection();
            SendRegisterMessage();
            ReceiveResponse();
        }

        protected SystemComponentType ParseType(string s)
        {
            switch(s)
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
        /// Metoda mająca na celu wysłanie odpowiedniego komnuniktatu w zalezności od urządzenia,
        /// na którym jest wywoływana.
        /// </summary>
        /// <param name="deviceType">Typ urządzenia rejestującego się.</param>

        private void SendRegisterMessage()
        {
            Register msg = RegisterGenerator.Generate(deviceType, solvableProblems, pararellThreads, false, null);
            try
            {
                SendMessage(msg);
            }
            catch(MessageNotSentException)
            {
                Console.WriteLine("Register Message Not Send");
            }
        }    

        /// <summary>
        /// Metoda używana do otrzymywania wiadomści, wyświetla na konsolę otrzymany message 
        /// </summary>
        protected void ReceiveResponse()
        {
            if (!tcpClient.Connected) tcpClient.Connect(communicationInfo.CommunicationServerAddress.Host, (int)communicationInfo.CommunicationServerPort);
            var stream= tcpClient.GetStream();
            byte[] byteArray = new byte[1024];
            try
            {
                stream.Read(byteArray, 0, 1024);
            }
            catch(Exception)
            {
                Console.WriteLine("Connection was killed by host");
                return;
            }
            String message = Message.Sanitize(byteArray);
            //Console.WriteLine(message);
            Validate(message, null); //Uważać z nullem w klasach dziedziczących
        }
        
        #region MessageGenerationAndHandling
        
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
                case Solution:
                    MsgHandler_Solution((Solutions)message);
                    return;
                default: throw new InvalidOperationException("Unknown msg key"); //TODO: własny wyjątek;
            }
        }

        protected virtual void MsgHandler_Solution(Solutions solutions)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Abstrakcyjna klasa do obsługi komunikatu o błędzie, obsługa zależna od odbiorcy.
        /// </summary>
        /// <param name="error"></param>
        protected virtual void MsgHandler_Error(Messages.Error message)
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
        /// Metoda reaguje na register response, tworząc  wątek, który regularnie co Timeout wysyła wiadomość
        /// typu Status Report do serwera
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
                    (o) =>
                    {
                        Console.WriteLine("Sending Status");
                    SendMessage(GenerateStatus());
                    Thread thread = new Thread(ReceiveResponse);
                    thread.IsBackground = true;
                    thread.Start() ;
                    },null, 0, (int)message.Timeout * MilisecondsMultiplier);
            }
            catch(NegativeIdException)
            {
                Console.WriteLine("Negative Id for component");
            }
            catch(MessageNotSentException)
            {
                Console.WriteLine("Message Not send for component type {0} with id {1}", deviceType.ToString(), this.Id);
            }
            
        }
        protected virtual Status GenerateStatus()
        {
            return StatusReportGenerator.Generate(Id, threadInfo.Threads);
        }
        
        /// <summary>
        /// Metoda reaguje na NoOperation, aktualizując dane o Backup Serwerze
        /// </summary>
        /// <param name="message"> Message typu NoOperation na który reagujemy</param>
        protected void MsgHandler_NoOperation(NoOperation message)
        {
            Console.WriteLine("NoOperation Message");
            BackupServer = message.BackupCommunicationServers.BackupCommunicationServer;
        }

        /// <summary>
        /// Metoda walidująca wiadomości z istniejącymi schemami
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
                //Console.WriteLine(message);
                if (!tcpClient.Connected)
                {
                    tcpClient.Connect(communicationInfo.CommunicationServerAddress.Host, (int)communicationInfo.CommunicationServerPort);
                }
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
