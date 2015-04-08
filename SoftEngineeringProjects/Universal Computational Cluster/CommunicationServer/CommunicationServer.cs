using Common.Messages;
using System;
using Common.Properties;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using Common.Exceptions;
using System.Text;
using System.Threading;

using System.Net;
using Common.Communication;
namespace Common.Components
{
    public class CommunicationServer : SystemComponent
    {
        bool isPrimary;
        #region Constants
        const int TimeoutModifier = 3;
        const String Register = "Register", Status = "Status";
        #endregion
        #region ComunicationData
        Dictionary<ulong, bool> TimerStoppers;
        Dictionary<ulong, System.Timers.Timer> Timers;
        Dictionary<ulong, Socket> Sockets;
        #endregion
        ulong FirstFreeID;
         public List<CommunicationInfo> CommunicationInfos;
        public CommunicationServer(bool primary=true)
            : base()
        {
            isPrimary = primary;
            FirstFreeID = 0;
            CommunicationInfos = new List<Communication.CommunicationInfo>();
            Timers = new Dictionary<ulong, System.Timers.Timer>();
            TimerStoppers = new Dictionary<ulong, bool>();
            Sockets = new Dictionary<ulong, Socket>();

            deviceType = SystemComponentType.CommunicationServer;
            solvableProblems = new string[] {"DVRP"};
            pararellThreads = 1;
        }

        /// <summary>
        /// Inicjalizacja słowników typami wiadomościami unikalnymi dla CS.
        /// </summary>
        protected override void Initialize()
        {
            //Inicjalizacja
            base.Initialize();
            //Register
            SchemaTypes.Add(Register, new Tuple<string, Type>(Resources.Register, typeof(Register)));
            //Status
            SchemaTypes.Add(Status, new Tuple<string, Type>(Resources.Status, typeof(Status)));
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
            foreach (var CI in CommunicationInfos)
            {

                Thread listener = new Thread(new ParameterizedThreadStart(StartListening));
                listener.IsBackground = true;
                listener.Start((object)CI);
            }
        }

        /// <summary>
        /// Metoda obsługi kolejki wiadomości odbierającej komunikaty
        /// </summary>
        private void MessageQueueWork()
        {
            while (true)
            {
                MessageQueueMutex.WaitOne();
                MessageQueueMutex.Reset();
                while (MessageQueue.Count > 0)
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

        /// <summary>
        /// Override metody komponentu rozpoczynająca nasłuchiwania serwera
        /// </summary>
        public override void Start()
        {
            InitializeMessageQueue((int)communicationInfo.CommunicationServerPort);
        }
        private void OnTimedEvent(int k)
        {

        }


        /// <summary>
        /// Metoda nawiązująca połączenie z nadającym wiadomości komponentem.
        /// </summary>
        public void StartListening(object communicationInfo)
        {
            var CommInfo = (CommunicationInfo)communicationInfo;
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse(CommInfo.CommunicationServerAddress.Host);
            TcpListener tcpListener = new TcpListener(ipAddress, (int)CommInfo.CommunicationServerPort);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, (int)CommInfo.CommunicationServerPort);
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
        
        /// <summary>
        /// Metoda stworzona na potrzeby testów, służąca do raportowania przesłanych bajtów.
        /// </summary>
        /// <param name="ar">Parametr Callbacku, z którego uzyskujemy socket.</param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                int bytesSent = handler.EndSend(ar);
                // na potrzeby testów
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #region MessageGenerationAndHandling

        /// <summary>
        /// Override metody z SystemComponentu, do obsługi wiadomości, rozszerzona o wiadomości unikalne dla tego komponentu.
        /// </summary>
        /// <param name="message">Otrzymana wiadomość.</param>
        /// <param name="key">Nazwa schemy której otrzymujemy.</param>
        /// <param name="socket">Socket z którego przyszła wiadomość.</param>
        protected override void HandleMessage(Messages.Message message, string key, Socket socket)
        {
            
            switch (key)
            {
                case Register:
                    MsgHandler_Register((Register)message, socket);
                    return;
                case Status:
                    MsgHandler_Status((Status)message);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }

        }

        /// <summary>
        /// Obsługa otrzymanego komunikatu o błędzie
        /// </summary>
        /// <param name="message">Otrzymany komunikat o błędzie</param>
        protected override void MsgHandler_Error(Error message)
        {
            if (!isPrimary)
            {
                base.MsgHandler_Error(message);
                return;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Metoda obsługująca otrzymaną wiadomość typu status.
        /// </summary>
        /// <param name="status">Otrzymany status.</param>
        private void MsgHandler_Status(Status status)
        {
            TimerStoppers[status.Id] = true;
            SendMessageToComponent(status.Id, GenerateNoOperationMessage());
        }
        
        /// <summary>
        /// Metoda obsługująca otrzymaną Register message.
        /// </summary>
        /// <param name="register">Otrzymana wiadomość.</param>
        /// <param name="socket">Socket na którym ją otrzymaliśmy.</param>
        private void MsgHandler_Register(Register register, Socket socket)
        {
            if (register.DeregisterSpecified)
            {
                if (register.Deregister)
                {
                    Deregister(register.Id);
                    return;
                }
            }
            //TODO: sprawdzenie, czy już nie jest zarejestrowany

            ulong id = FirstFreeID++;
            Sockets.Add(id, socket);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            TimerStoppers.Add(id, true);
            var Timer = new System.Timers.Timer();
            Timer.Elapsed += (u,t) =>
            {
                if (!TimerStoppers[(ulong)id]) Deregister((ulong)id);
                else TimerStoppers[(ulong)id] = false;
            };
            Timer.Interval = TimeoutModifier * (uint)CommunicationInfo.Time;
  //          Timer.Enabled = true;
    //        Timer.AutoReset = true;
            Timers.Add(Id, Timer);
           // Timer.Start();
            RegisterResponse response = new RegisterResponse();
            response.Id = id;
            response.Timeout = TimeoutModifier * (uint)CommunicationInfo.Time;
            response.BackupCommunicationServers = new RegisterResponseBackupCommunicationServers();
            response.BackupCommunicationServers.BackupCommunicationServer =
                new RegisterResponseBackupCommunicationServersBackupCommunicationServer();
            if (BackupServer != null)
            {
                response.BackupCommunicationServers.BackupCommunicationServer.address =
                    BackupServer.address;
                response.BackupCommunicationServers.BackupCommunicationServer.port =
                    BackupServer.port;
                response.BackupCommunicationServers.BackupCommunicationServer.portSpecified =
                    BackupServer.portSpecified;
            }
            SendMessageToComponent(id, response);
        }

        /// <summary>
        /// Metoda usuwająca komponent i zrywająca z nim połączenie.
        /// </summary>
        /// <param name="Id">ID komponentu do derejestracji.</param>
        /// <returns></returns>
        private bool Deregister(ulong Id)
        {
            return false;
            if (!Sockets.ContainsKey(Id)) return false;
            
            Sockets[Id].Close();
            Sockets.Remove(Id);
            Timers[Id].Enabled = false;
            Timers.Remove(Id);
            TimerStoppers.Remove(Id);
          
            return true;
        }

        /// <summary>
        /// Metoda generująca nooperation message do odesłania do komponentu.
        /// </summary>
        /// <returns>NoOperation message zawierający dane o backupach.</returns>
        private NoOperation GenerateNoOperationMessage()
        {
            NoOperation Noop = new NoOperation();
            Noop.BackupCommunicationServers = new NoOperationBackupCommunicationServers();
            Noop.BackupCommunicationServers.BackupCommunicationServer = BackupServer;
            return Noop;
        }
        #endregion
        #region ConnectionHandling
        /// <summary>
        /// Metoda wysyłająca wiadomość do komponentu.
        /// </summary>
        /// <param name="Id">Komponent do którego wysyłamy wiadomość.</param>
        /// <param name="message">Wiadomość do wysłaniu.</param>
        protected void SendMessageToComponent(ulong Id, Message message)
        {
            try
            {
                var socket = Sockets[Id]; // Jeśli Id jest nieznane patrz Error Message
                Console.WriteLine(message.GetType().ToString());
                byte[] byteData = Encoding.ASCII.GetBytes(message.toString());
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);
            }
            catch (Exception e)
            {
                String exceptionMessage = "Unable to send message";
                throw new MessageNotSentException(exceptionMessage, e);
            }
        }
        #endregion

        public void InitializeIPList()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.IsIPv6LinkLocal) continue;
                var localIP = ip.ToString();
                    if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        localIP = "[" + localIP + "]";
                    }
                   localIP = "http://" + localIP + "/";
                    var C = new CommunicationInfo();
                    C.CommunicationServerPort = CommunicationInfo.CommunicationServerPort;
                    C.CommunicationServerAddress = new Uri(localIP);
                    CommunicationInfos.Add(C);
                    
                
            }
            
        }
    }
}
