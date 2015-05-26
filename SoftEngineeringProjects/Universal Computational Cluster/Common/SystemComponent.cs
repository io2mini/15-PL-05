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
using System.Net;

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
        // reprezentuje informacje o aktualnym primary communication server
        public CommunicationInfo CommunicationServerInfo { get; set; }
        public SystemComponentType DeviceType { get; protected set; }
        protected byte PararellThreads;
        protected string[] SolvableProblems;
        protected TcpClient TcpClient;
        protected ThreadInfo ThreadInfo;
        protected List<Tuple<string, ushort>> BackupCommunicationServers;
        // musi się tutaj znajodwać bo mamy po części obslugę buckupa gdy prmary cs się posypie w tej klasie
        public CommunicationInfo MyCommunicationInfo { get; set; }


        protected SystemComponent()
        {
            IsWorking = true;
            /*
             * TODO: Initialize Thread Array
             */
            PararellThreads = 1;
            ThreadInfo = new ThreadInfo(PararellThreads, this);
            BackupCommunicationServers = new List<Tuple<String, ushort>>();
            Initialize();
        }

        protected ulong Id { get; set; }
        public bool IsWorking { get; set; }

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
            InitializeConnection(); //TODO: sypie się przy nieudanej próbie
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
        protected void SendRegisterMessage()
        {
            var msg = RegisterGenerator.Generate(DeviceType, SolvableProblems, PararellThreads);
            try
            {
                SendMessage(msg);
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine(Resources.SystemComponent_SendRegisterMessage_Register_Message_Not_Send);
            }
        }

        /// <summary>
        ///     Metoda wysyłajaca Error Message
        /// </summary>
        /// <param name="message">wiadomosć przekazywana w Error Message</param>
        /// <param name="errorType">typ błędu do wygenerowania</param>
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
                TcpClient.Connect(CommunicationServerInfo.CommunicationServerAddress.Host,
                    CommunicationServerInfo.CommunicationServerPort);
            var stream = TcpClient.GetStream();
            var byteArray = new byte[1024];
            try
            {
                stream.Read(byteArray, 0, 1024);
            }
            catch (Exception)
            {
                Console.WriteLine(Resources.SystemComponent_ReceiveResponse_Connection_was_killed_by_host);
                return;
            }
            var message = Message.Sanitize(byteArray);
            //Console.WriteLine("Received "+message.GetType());
            Validate(message, null); //Uważać z nullem w klasach dziedziczących
        }

        #region Constants

        protected const string RegisterResponse = "RegisterResponse",
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
        protected virtual void MsgHandler_Error(Error message)
        {
            Console.WriteLine(Resources.SystemComponent_MsgHandler_Error_Error_Message);
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
                        ThreadInfo.SetStateAll(StatusThreadState.Idle);
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
            Console.WriteLine(Resources.SystemComponent_MsgHandler_RegisterResponse_, message.Id);
            CommunicationServerInfo.Time = message.Timeout;
            BackupCommunicationServers.Clear();
            if (message.BackupCommunicationServers != null)
            {
                RegisterResponseBackupCommunicationServer[] bServers = message.BackupCommunicationServers;
                for (int i = 0; i < bServers.Length; i++)
                {
                    if (bServers[i].portSpecified)
                    {
                        BackupCommunicationServers.Add(new Tuple<string, ushort>
                        (bServers[i].address, bServers[i].port));
                    }
                }
            }
            Id = message.Id;
            try
            {
                StatusReporter = new Timer(
                    o =>
                    {

                        Console.WriteLine(Resources.SystemComponent_MsgHandler_RegisterResponse_Sending_Status);
                        SendMessage(GenerateStatus());
                        var thread = new Thread(ReceiveResponse);
                        thread.IsBackground = true;
                        thread.Start();
                    }, null, 0, (int)message.Timeout * MilisecondsMultiplier);
            }
            catch (InvalidIdException)
            {
                Console.WriteLine(Resources.SystemComponent_MsgHandler_RegisterResponse_Negative_Id_for_component);
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine(Resources.SystemComponent_MsgHandler_RegisterResponse_Message_Not_send_for_component_type__0__with_id__1_, DeviceType, Id);
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
            Console.WriteLine(Resources.SystemComponent_MsgHandler_NoOperation_NoOperation_Message);
            BackupCommunicationServers.Clear();
            if (message.BackupCommunicationServers != null)
            {
                NoOperationBackupCommunicationServer[] bServers = message.BackupCommunicationServers;
                for (int i = 0; i < bServers.Length; i++)
                {
                    if (bServers[i].portSpecified)
                    {
                        BackupCommunicationServers.Add(new Tuple<string, ushort>
                        (bServers[i].address, bServers[i].port));
                    }
                }
            }
        }

        /// <summary>
        ///     Metoda walidująca wiadomości z istniejącymi schemami
        /// </summary>
        /// <param name="xml">wiadomość w postaci łańcucha znaków</param>
        /// <param name="socket">Socket z którego otrzymano</param>
        protected void Validate(string xml, Socket socket)
        {
            XDocument message;
            try
            {
                message = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            }
            catch (XmlException)
            {
                //TODO: Log warning
                return;
            }
            foreach (var key in SchemaTypes.Keys)
            {
                var schemas = new XmlSchemaSet();
                schemas.Add(null, XmlReader.Create(new StringReader(SchemaTypes[key].Item1)));
                var errorOccured = false;
                message.Validate(schemas, (o, e) => { errorOccured = true; });
                if (!errorOccured)
                {
                    var parsedMsg = Message.ParseXML(SchemaTypes[key].Item2, xml);
                    Console.WriteLine("Received "+parsedMsg.GetType());
                    HandleMessage(parsedMsg, key, socket);
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
            var xmlSerializer = new XmlSerializer(typeof(CommunicationInfo));
            xmlSerializer.Serialize(new FileStream(path, FileMode.Create), CommunicationServerInfo);
        }

        /// <summary>
        ///     Metoda deserializująca informacje komunikacyjne z pliku
        /// </summary>
        /// <param name="path">Ścieżka pliku</param>
        public virtual void LoadConfig(string path)
        {
            var xmlDeSerializer = new XmlSerializer(typeof(CommunicationInfo));
            try
            {
                CommunicationServerInfo = (CommunicationInfo)xmlDeSerializer.Deserialize(new FileStream(path, FileMode.Open));
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
        protected virtual void InitializeConnection()
        {
            try
            {
                TcpClient = new TcpClient(CommunicationServerInfo.CommunicationServerAddress.Host,
                    CommunicationServerInfo.CommunicationServerPort);
                TcpClient.ReceiveBufferSize = 10000;
                TcpClient.SendBufferSize = 10000;

            }
            catch (SocketException e)
            {
                var message = string.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    CommunicationServerInfo.CommunicationServerAddress.Host, CommunicationServerInfo.CommunicationServerPort);
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
                    TcpClient.Connect(CommunicationServerInfo.CommunicationServerAddress.Host,
                        CommunicationServerInfo.CommunicationServerPort);
                }
                var stream = TcpClient.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) {AutoFlush = false};
                writer.Write(message);
                writer.Flush();
            }
            catch (IOException)
            {
                // disconnected from primary CS, set buckup as primary
                if (BackupCommunicationServers.Count > 0)
                {
                    CommunicationServerInfo.CommunicationServerPort = BackupCommunicationServers[0].Item2;
                    CommunicationServerInfo.CommunicationServerAddress = new Uri(BackupCommunicationServers[0].Item1);
                    // zarejestruj się ponownie u innego cs'a
                    if (DeviceType.Equals(SystemComponentType.CommunicationServer)
                        && MyCommunicationInfo.CommunicationServerAddress == CommunicationServerInfo.CommunicationServerAddress
                        && MyCommunicationInfo.CommunicationServerPort == CommunicationServerInfo.CommunicationServerPort)
                    {
                        // jeżeli jesteś backupem, który powinien być teraz Cs'em nie wysyłaj do siebie wiadomości
                        // czekaj aż ktoś spróbuje coś do Ciebie wysłać wtedy automatycznie zamienisz się na primary CS
                        // jeżeli jesteś nie wykorzystywanem backupem zarejestruj się
                        MyCommunicationInfo.IsBackup = false;
                        CommunicationServerInfo.IsBackup = false;
                    }
                    else
                    {
                        SendRegisterMessage();
                    }
                }
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