using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.Configuration;
using Common.Exceptions;
using Common.Messages;
using Common.Properties;
using Timer = System.Timers.Timer;

namespace Common.Components
{
    public class CommunicationServer : SystemComponent
    {
        #region Constants

        private const int TimeoutModifier = 3000;

        private const string Register = "Register",
            Status = "Status",
            SolveRequest = "SolveRequest",
            PartialProblems = "PartialProblems",
            SolutionRequest = "SolutionRequest";

        #endregion

        public bool IsPrimary { get; set; }
        private ulong _firstFreeComponentId;
        private ulong _firstFreeProblemId;
        // wszystkie adresy, na których nasłuchuje server, to są jego adresy fizyczne
        public List<CommunicationInfo> CommunicationInfos;

        #region ComunicationData

        private readonly Dictionary<ulong, StatusThread[]> _threadStates;
        private readonly Dictionary<ulong, bool> _timerStoppers;
        private readonly Dictionary<ulong, Timer> _timers;
        private readonly Dictionary<ulong, Socket> _idToSocket;
        private readonly Dictionary<Socket, ulong> _socketToId;
        private readonly Dictionary<ulong, byte[]> _problems;
        private readonly Dictionary<ulong, SolvePartialProblems> _savedPartialProblems;
        private readonly Dictionary<ulong, SystemComponentType> _componentTypes;
        private readonly Dictionary<ulong, List<string>> _solvableProblemTypes;

        #endregion

        public CommunicationServer()
        {
            CommunicationInfos = new List<CommunicationInfo>();
            DeviceType = SystemComponentType.CommunicationServer;
            _firstFreeProblemId = _firstFreeComponentId = 0;
            _timers = new Dictionary<ulong, Timer>();
            _timerStoppers = new Dictionary<ulong, bool>();
            _idToSocket = new Dictionary<ulong, Socket>();
            _socketToId = new Dictionary<Socket, ulong>();
            _problems = new Dictionary<ulong, byte[]>();
            _threadStates = new Dictionary<ulong, StatusThread[]>();
            _savedPartialProblems = new Dictionary<ulong, SolvePartialProblems>();
            _componentTypes = new Dictionary<ulong, SystemComponentType>();
            _solvableProblemTypes = new Dictionary<ulong, List<string>>();
            SolvableProblems = new[] { "DVRP" }; //To pole dotyczy node'ów, sprawdzić w dokumentacji czy trzeba je inicjalizować w CS
        }

        /// <summary>
        ///     Inicjalizacja słowników typami wiadomościami unikalnymi dla CS.
        /// </summary>
        protected override void Initialize()
        {
            //Inicjalizacja
            base.Initialize();
            //Register
            SchemaTypes.Add(Register, new Tuple<string, Type>(Resources.Register, typeof(Register)));
            //Status
            SchemaTypes.Add(Status, new Tuple<string, Type>(Resources.Status, typeof(Status)));
            //SolveRequest
            SchemaTypes.Add(SolveRequest, new Tuple<string, Type>(Resources.SolveRequest, typeof(SolveRequest)));
            //PartialProblems
            SchemaTypes.Add(PartialProblems,
                new Tuple<string, Type>(Resources.PartialProblems, typeof(SolvePartialProblems)));
            //SolutionRequest
            SchemaTypes.Add(SolutionRequest,
                new Tuple<string, Type>(Resources.SolutionRequest, typeof(SolutionRequest)));
        }

        /// <summary>
        ///     Metoda inicializująca kolejkę wiadomości do odbioru
        /// </summary>
        protected void InitializeMessageQueue(int port)
        {
            MessageQueueMutex = new AutoResetEvent(true);
            MessageQueue = new Queue<Tuple<string, Socket>>();
            var thread = new Thread(MessageQueueWork);
            thread.IsBackground = true;
            thread.Start();
            foreach (var ci in CommunicationInfos)
            {
                var listener = new Thread(StartListening);
                listener.IsBackground = true;
                listener.Start(ci);
            }
        }

        /// <summary>
        ///     Override metody komponentu rozpoczynająca nasłuchiwania serwera
        /// </summary>
        public override void Start()
        {
            if (IsPrimary)
            {
                InitializeMessageQueue(CommunicationServerInfo.CommunicationServerPort);
            }
            else
            {
                // jesteśmy backupem
                InitializeConnection();
                SendRegisterMessage();
                ReceiveResponse();
            }
            while (IsWorking)
            {
            }
        }

        #region InnerDataFunctions

        /// <summary>
        ///     Metoda obsługi kolejki wiadomości odbierającej komunikaty
        /// </summary>
        private void MessageQueueWork()
        {
            while (true)
            {
                MessageQueueMutex.WaitOne();
                MessageQueueMutex.Reset();
                while (MessageQueue.Count > 0)
                {
                    var message = MessageQueue.Dequeue();
                    //Console.WriteLine(Message.Item1);
                    Validate(message.Item1, message.Item2);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        ///     Funkcja wyszukująca TM.
        /// </summary>
        /// <param name="problemType">Żądany rodzaj problemu.</param>
        /// <returns>Id znalezionego TM.</returns>
        private ulong GetTm(string problemType)
        {
            foreach (
                var key in
                    _threadStates.Keys.Where(
                        key =>
                            _componentTypes[key] == SystemComponentType.TaskManager &&
                            _solvableProblemTypes[key].Contains(problemType))
                        .Where(key => _threadStates[key].Any(thread => thread.State == StatusThreadState.Idle)))
            {
                return key;
            }
            throw new Exception("TM not found"); //TODO: create new exception
        }

        /// <summary>
        ///     Funkcja obliczająca ilość dostępnych threadów.
        /// </summary>
        /// <returns>Ilość dostępnych threadów.</returns>
        private ulong GetAvailableThreads(string problemType)
        {
            return (ulong)(from key in _threadStates.Keys
                           where
                               _componentTypes[key] == SystemComponentType.ComputationalNode &&
                               _solvableProblemTypes[key].Contains(problemType)
                           from thread in _threadStates[key]
                           where thread.State == StatusThreadState.Idle
                           select thread).Count();

        }

        /// <summary>
        ///     Zwraca listę wolnych CNów mogących zająć się zadanym rodzajem problemu
        /// </summary>
        /// <param name="problemType"></param>
        /// <param name="neededNodes"></param>
        /// <returns></returns>
        private Dictionary<ulong, int> GetComputationNodes(string problemType, int neededNodes)
        {
            var l = new Dictionary<ulong, int>();
            var i = 0;
            foreach (var key in from key in _threadStates.Keys
                                where
                                    _componentTypes[key] == SystemComponentType.ComputationalNode &&
                                    _solvableProblemTypes[key].Contains(problemType)
                                from thread in _threadStates[key]
                                where thread.State == StatusThreadState.Idle
                                select key)
            {
                if (l.ContainsKey(key))
                    l[key]++;
                else
                    l.Add(key, 1);
                i++;
                if (i >= neededNodes)
                {
                    break;
                }
            }
            return l;
        }

        private void ChangeServerFromBuckupToPrimary()
        {
            MyCommunicationInfo.IsBackup = false;
            CommunicationServerInfo = MyCommunicationInfo;
            IsPrimary = true;
            Console.WriteLine("Server is now primary not backup");
            InitializeMessageQueue(CommunicationServerInfo.CommunicationServerPort);
            if (BackupCommunicationServers != null && BackupCommunicationServers.Count > 0)
            {
                BackupCommunicationServers.RemoveAll(x =>
                    (x.Item1 == CommunicationServerInfo.CommunicationServerAddress.Host
                     && x.Item2 == CommunicationServerInfo.CommunicationServerPort));
            }
        }

        #endregion

        #region MessageGenerationAndHandling

        /// <summary>
        ///     Override metody z SystemComponentu, do obsługi wiadomości, rozszerzona o wiadomości unikalne dla tego komponentu.
        /// </summary>
        /// <param name="message">Otrzymana wiadomość.</param>
        /// <param name="key">Nazwa schemy której otrzymujemy.</param>
        /// <param name="socket">Socket z którego przyszła wiadomość.</param>
        protected override void HandleMessage(Message message, string key, Socket socket)
        {
            //Tryb niekompatybilny
            switch (key)
            {
                case Register:
                    if (!IsPrimary) ChangeServerFromBuckupToPrimary(); //TODO: poprawić!!!!!!!!!!!!!!!
                    MsgHandler_Register((Register)message, socket);
                    return;
                case Status:
                    if (!IsPrimary) ChangeServerFromBuckupToPrimary();
                    MsgHandler_Status((Status)message, socket);
                    return;
                case SolveRequest:
                    if (!IsPrimary) ChangeServerFromBuckupToPrimary();
                    MsgHandler_SolveRequest((SolveRequest)message, socket);
                    return;
                case PartialProblems:
                    if (!IsPrimary) ChangeServerFromBuckupToPrimary();
                    MsgHandler_PartialProblems((SolvePartialProblems)message, socket);
                    return;
                case SolutionRequest:
                    if (!IsPrimary) ChangeServerFromBuckupToPrimary();
                    MsgHandler_SolutionRequest((SolutionRequest)message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
            //TODO: Zrobić tryb kompatybilny
        }

        /// <summary>
        ///     Obsługa otrzymanego żądania rozwiązania
        /// </summary>
        /// <param name="solutionRequest"></param>
        /// <param name="socket"></param>
        private void MsgHandler_SolutionRequest(SolutionRequest solutionRequest, Socket socket)
        {
            //TODO: send solutionMessage
            //throw new NotImplementedException();
        }

        /// <summary>
        ///     Obsługa otrzymanego komunikatu na temat rozwiązań
        /// </summary>
        /// <param name="solutions"></param>
        protected override void MsgHandler_Solution(Solutions solutions)
        {
            //TODO: save solution
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Obsługa otrzymanego podzielonego problemu
        /// </summary>
        /// <param name="solvePartialProblems"></param>
        /// <param name="socket"></param>
        protected void MsgHandler_PartialProblems(SolvePartialProblems solvePartialProblems, Socket socket)
        {
            var nodes = GetComputationNodes(solvePartialProblems.ProblemType,
                solvePartialProblems.PartialProblems.Count());
            var ind = 0;
            foreach (var id in nodes.Keys)
            {
                for (var i = 0; i < nodes[id]; i++)
                {
                    solvePartialProblems.PartialProblems[ind].NodeID = id;
                    ind++;
                }
            }
            if (ind < solvePartialProblems.PartialProblems.Count())
                throw new Exception("Not enough nodes"); //TODO: handle exception; 
            _savedPartialProblems.Add(solvePartialProblems.Id, solvePartialProblems);

            var toSend = new Dictionary<ulong, SolvePartialProblems>();
            foreach (var id in nodes.Keys)
            {
                var msg = new SolvePartialProblems
                {
                    ProblemType = solvePartialProblems.ProblemType,
                    Id = solvePartialProblems.Id,
                    SolvingTimeoutSpecified = solvePartialProblems.SolvingTimeoutSpecified,
                    SolvingTimeout = solvePartialProblems.SolvingTimeout,
                    CommonData = solvePartialProblems.CommonData,
                    PartialProblems = solvePartialProblems.PartialProblems.Where(spppp => spppp.NodeID == id).ToArray()
                };
                toSend.Add(id, msg);
            }

            foreach (var id in toSend.Keys)
            {
                SendMessageToComponent(id, toSend[id]);
            }
        }

        /// <summary>
        ///     Obsługa otrzymanego żadania rozwiązania
        /// </summary>
        /// <param name="solveRequest">Otrzymana wiadomość</param>
        /// <param name="socket">Skąd otrzymana</param>
        protected void MsgHandler_SolveRequest(SolveRequest solveRequest, Socket socket)
        {
            var problemId = _firstFreeProblemId++;
            /*
             * TODO:
             * 4. If specified, handle solve timeout.
             */
            _problems.Add(problemId, solveRequest.Data);

            var response = new SolveRequestResponse { Id = problemId };
            SendMessageToComponent(socket, response);

            var msg = new DivideProblem
            {
                Data = solveRequest.Data,
                Id = problemId,
                ProblemType = solveRequest.ProblemType,
                ComputationalNodes = GetAvailableThreads(solveRequest.ProblemType)
            };
            var tmId = GetTm(solveRequest.ProblemType);
            msg.NodeID = tmId;

            SendMessageToComponent(tmId, msg);

            //TODO:Handle timeout if specified
        }

        /// <summary>
        ///     Obsługa otrzymanego komunikatu o błędzie
        /// </summary>
        /// <param name="message">Otrzymany komunikat o błędzie</param>
        protected override void MsgHandler_Error(Error message)
        {
            if (IsPrimary && message.ErrorType == ErrorErrorType.ExceptionOccured) throw new NotImplementedException();
            base.MsgHandler_Error(message);
            //TODO: Handle Exception
        }

        /// <summary>
        ///     Metoda obsługująca otrzymaną wiadomość typu status.
        /// </summary>
        /// <param name="status">Otrzymany status.</param>
        /// <param name="sender">Socket na którym go otrzymaliśmy.</param>
        protected void MsgHandler_Status(Status status, Socket sender)
        {
            Console.WriteLine(Resources.CommunicationServer_MsgHandler_Status_, status.Id, NoOperation);
            if (!_idToSocket.ContainsKey(status.Id))
            {
                var err = new Error { ErrorType = ErrorErrorType.UnknownSender };
                SendMessageToComponent(sender, err);
                return;
            }
            _timerStoppers[status.Id] = true;
            _threadStates[status.Id] = status.Threads;
            SendMessageToComponent(status.Id, GenerateNoOperationMessage());
        }

        /// <summary>
        /// Metoda inicjalizująca połączenie buckupa do serwera
        /// </summary>
        protected override void InitializeConnection()
        {
            try
            {
                //TcpClient = new TcpClient(CommunicationServerInfo.CommunicationServerAddress.Host,
                //    CommunicationServerInfo.CommunicationServerPort);
                IPAddress localIpAddress = null;
                if (IPAddress.TryParse("127.0.0.1", out localIpAddress))
                {
                    IPEndPoint ipLocalEndPoint = new IPEndPoint(localIpAddress, MyCommunicationInfo.CommunicationServerPort);
                    TcpClient = new TcpClient(ipLocalEndPoint);
                    IPAddress remoteIpAddress = null;
                    if (IPAddress.TryParse(CommunicationServerInfo.CommunicationServerAddress.Host, out remoteIpAddress))
                    {
                        IPEndPoint remoteEndpoint = new IPEndPoint(remoteIpAddress,
                        CommunicationServerInfo.CommunicationServerPort);
                        TcpClient.Connect(remoteEndpoint);
                    }
                }
            }
            catch (SocketException e)
            {
                var message = string.Format("Problems with connecting to Communication Server host: {0} ; port: {1}",
                    CommunicationServerInfo.CommunicationServerAddress.Host, CommunicationServerInfo.CommunicationServerPort);
                throw new ConnectionException(message, e);
            }
        }

        /// <summary>
        ///     Metoda obsługująca otrzymaną Register message.
        /// </summary>
        /// <param name="register">Otrzymana wiadomość.</param>
        /// <param name="socket">Socket na którym ją otrzymaliśmy.</param>
        protected void MsgHandler_Register(Register register, Socket socket)
        {
            // rejestruje komponenty tylko primary
            if (!IsPrimary) return;
            if (register.DeregisterSpecified)
            {
                if (register.Deregister)
                {
                    Deregister(register.Id);
                    return;
                }
            }
            //TODO: sprawdzenie, czy już nie jest zarejestrowany
            var id = _firstFreeComponentId++;
            Console.WriteLine(Resources.CommunicationServer_MsgHandler_Register_, id);
            GenerateBackupCommunicationServerList(); //Czemu dzieje się to tutaj a nie wcześniej?
            var response = new RegisterResponse
            {
                Id = id,
                Timeout = (uint)CommunicationServerInfo.Time,
                BackupCommunicationServers = BackupCommunicationServers.Select(t => new RegisterResponseBackupCommunicationServer
                {
                    address = t.Item1, port = t.Item2, portSpecified = true
                }).ToArray()
            };
            var timer = RegisterComponent(socket, id, ParseType(register.Type), register.SolvableProblems);
            SendMessageToComponent(id, response);

            timer.Enabled = true;
            timer.Start();
            timer.AutoReset = true;
        }

        protected Timer RegisterComponent(Socket socket, ulong id, SystemComponentType type, string[] solvableProbs)
        {
            _idToSocket.Add(id, socket);
            _socketToId.Add(socket, id);
            _componentTypes.Add(id, type);
            _threadStates.Add(id, null); //Czy null jest dobrym state'm?
            if (SolvableProblems.Any())
            {
                _solvableProblemTypes.Add(id, solvableProbs.ToList());
            }
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _timerStoppers.Add(id, true);
            var timer = new Timer();
            timer.Elapsed += (u, t) =>
            {
                if (!_timerStoppers[id]) Deregister(id);
                else _timerStoppers[id] = false;
            };
            timer.Interval = TimeoutModifier * (uint)CommunicationServerInfo.Time;
            _timers.Add(id, timer);
            return timer;
        }

        /// <summary>
        ///     Metoda zamykająca juz nie uzywany socket
        /// </summary>
        /// <param name="socket">socket, który nalezy zamknąć</param>
        protected void CloseSocket(object socket)
        {
            Thread.Sleep(new TimeSpan(0, 0, 15, 0));
            if (socket is Socket) ((Socket)socket).Close(); //TODO: jeśli socket nie jest typu Socket rzuć wyjątkiem, wąchaj smar
        }

        /// <summary>
        ///     Metoda usuwająca komponent i zrywająca z nim połączenie.
        /// </summary>
        /// <param name="s">Socket komponentu do derejestracji</param>
        /// <returns></returns>
        protected bool Deregister(Socket s)
        {
            return Deregister(_socketToId[s]);
        }

        /// <summary>
        ///     Metoda usuwająca komponent i zrywająca z nim połączenie.
        /// </summary>
        /// <param name="id">ID komponentu do derejestracji.</param>
        /// <returns></returns>
        protected bool Deregister(ulong id)
        {
            //return false;
            if (!_idToSocket.ContainsKey(id)) return false;

            var socketCloser = new Thread(CloseSocket);
            socketCloser.IsBackground = true;
            socketCloser.Start(_idToSocket[id]);

            _socketToId.Remove(_idToSocket[id]);
            _idToSocket.Remove(id);
            _componentTypes.Remove(id);
            _threadStates.Remove(id);
            _solvableProblemTypes.Remove(id);

            _timers[id].Enabled = false;
            _timers[id].Close();
            _timers.Remove(id);
            _timerStoppers.Remove(id);
            _componentTypes.Remove(id);
            Console.WriteLine(Resources.CommunicationServer_Deregister_, id);
            return true;
        }

        /// <summary>
        /// Utwórz listę par [host, port] reprezentujących backupCommunicationServers
        /// </summary>
        private void GenerateBackupCommunicationServerList()
        {
            ulong[] backupServersIds = _componentTypes.Keys.Where
                (x => (_componentTypes[x] == SystemComponentType.CommunicationServer
                    && _idToSocket.ContainsKey(x) && _socketToId.ContainsKey(_idToSocket[x])))
                    .ToArray<ulong>();
            BackupCommunicationServers.Clear();
            for (int i = 0; i < backupServersIds.Length; i++)
            {
                Socket backupServer = _idToSocket[backupServersIds[i]];
                IPEndPoint ipEndPoint = (IPEndPoint)backupServer.RemoteEndPoint;
                IPHostEntry hostEntry = Dns.GetHostEntry(ipEndPoint.Address);
                string hostName = hostEntry.HostName;
                ushort port = (ushort)ipEndPoint.Port;
                BackupCommunicationServers.Add(new Tuple<string, ushort>(hostName, port));
            }
        }

        /// <summary>
        ///     Metoda generująca nooperation message do odesłania do komponentu.
        /// </summary>
        /// <returns>NoOperation message zawierający dane o backupach.</returns>
        private NoOperation GenerateNoOperationMessage()
        {

            GenerateBackupCommunicationServerList();
            //Tryb niekompatybilny
            var noop = new NoOperation
            {
                BackupCommunicationServers = BackupCommunicationServers.Select(t => new NoOperationBackupCommunicationServer
                {
                    address = t.Item1, port = t.Item2, portSpecified = true
                }).ToArray()
            };
            //TODO: tryb kompatybilny
            return noop;
        }

        #endregion

        #region ConnectionHandling

        /// <summary>
        ///     Metoda wysyłająca wiadomość do komponentu.
        /// </summary>
        /// <param name="id">Komponent do którego wysyłamy wiadomość.</param>
        /// <param name="message">Wiadomość do wysłania.</param>
        protected void SendMessageToComponent(ulong id, Message message)
        {
            SendMessageToComponent(_idToSocket[id], message);
        }

        /// <summary>
        ///     Metoda wysyłająca wiadomość do komponentu.
        /// </summary>
        /// <param name="receiver">Komponent do którego wysyłamy wiadomość.</param>
        /// <param name="message">Wiadomość do wysłania.</param>
        protected void SendMessageToComponent(Socket receiver, Message message)
        {
            try
            {
                Console.WriteLine(message.GetType().ToString());
                var byteData = Encoding.ASCII.GetBytes(message.toString());
                receiver.BeginSend(byteData, 0, byteData.Length, 0,
                    SendCallback, receiver);
            }
            catch (Exception e)
            {
                var exceptionMessage = "Unable to send message";
                throw new MessageNotSentException(exceptionMessage, e);
            }
        }

        /// <summary>
        ///     Inicjalizuje listę adresów do nasłuchiwania
        /// </summary>
        public void InitializeIpList()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var local = new CommunicationInfo
            {
                CommunicationServerAddress = new Uri("http://127.0.0.1/"),
                CommunicationServerPort = CommunicationServerInfo.CommunicationServerPort
            };
            CommunicationInfos.Add(local);
            foreach (var ip in host.AddressList)
            {
                if (ip.IsIPv6LinkLocal) continue;
                var localIp = ip.ToString();
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    localIp = "[" + localIp + "]";
                }
                localIp = "http://" + localIp + "/";
                var c = new CommunicationInfo
                {
                    CommunicationServerPort = CommunicationServerInfo.CommunicationServerPort,
                    CommunicationServerAddress = new Uri(localIp)
                };
                CommunicationInfos.Add(c);
            }
        }

        /// <summary>
        ///     Metoda nawiązująca połączenie z nadającym wiadomości komponentem.
        /// </summary>
        private void StartListening(object communicationInfo)
        {
            var commInfo = (CommunicationInfo)communicationInfo;
            var ipAddress = IPAddress.Parse(commInfo.CommunicationServerAddress.Host);
            var tcpListener = new TcpListener(ipAddress, commInfo.CommunicationServerPort);
            
            try
            {
                tcpListener.Start();
                while (IsWorking)
                {
                    Console.WriteLine(Resources.CommunicationServer_StartListening_Started_listening_on___0_, commInfo.CommunicationServerAddress);
                    var socket = tcpListener.AcceptSocket();
                    socket.ReceiveBufferSize = 10000;
                    Console.WriteLine(Resources.CommunicationServer_StartListening_Accepted_connection_from__0_, socket.RemoteEndPoint);
                    var thread = new Thread(ReceiveMessage) { IsBackground = true };
                    thread.Start(socket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        ///     Metoda pośrednia do obsługi nasłuchu.
        /// </summary>
        /// <param name="socket">sochet, na którym nasłuchujemy.</param>
        private void ReceiveMessage(object socket)
        {
            ReceiveMessage((Socket)socket);
        }

        /// <summary>
        ///     Metoda obsługująca nasłuchiwanie komunikatów od konkretnego komponentu.
        /// </summary>
        /// <param name="socket">Socket na którym odbywa się nasłuchiwanie.</param>
        private void ReceiveMessage(Socket socket)
        {
            try
            {
                while (socket.IsBound)
                {
                    var byteArray = new byte[10000];

                    Thread.Sleep(1000);
                    socket.Receive(byteArray);
                    var message = Message.Sanitize(byteArray);
                    MessageQueue.Enqueue(new Tuple<string, Socket>(message, socket));
                    MessageQueueMutex.Set();
                }
            }
            catch (SocketException e)
            {
                Debug.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        ///     Metoda stworzona na potrzeby testów, służąca do raportowania przesłanych bajtów.
        /// </summary>
        /// <param name="ar">Parametr Callbacku, z którego uzyskujemy socket.</param>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                var handler = (Socket)ar.AsyncState;
                var bytesSent = handler.EndSend(ar);
                // na potrzeby testów
                Console.WriteLine(Resources.CommunicationServer_SendCallback_Sent__0__bytes_to_client_, bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        public void Stop()
        {
            IsWorking = false;
        }
    }
}