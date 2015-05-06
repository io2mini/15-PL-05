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
        private readonly bool _isPrimary;
        private ulong _firstFreeComponentId;
        private ulong _firstFreeProblemId;
        public List<CommunicationInfo> CommunicationInfos;

        public CommunicationServer(Dictionary<ulong, SystemComponentType> componentTypes,
            Dictionary<ulong, List<string>> solvableProblemTypes, bool primary = true)
        {
            _componentTypes = componentTypes;
            _solvableProblemTypes = solvableProblemTypes;
            _isPrimary = primary;
            _firstFreeProblemId = _firstFreeComponentId = 0;
            CommunicationInfos = new List<CommunicationInfo>();
            _timers = new Dictionary<ulong, Timer>();
            _timerStoppers = new Dictionary<ulong, bool>();
            _idToSocket = new Dictionary<ulong, Socket>();
            _socketToId = new Dictionary<Socket, ulong>();
            _problems = new Dictionary<ulong, byte[]>();
            _threadStates = new Dictionary<ulong, StatusThread[]>();
            _savedPartialProblems = new Dictionary<ulong, SolvePartialProblems>();
            DeviceType = SystemComponentType.CommunicationServer;
            SolvableProblems = new[] {"DVRP"};
            PararellThreads = 1;
        }

        public CommunicationServer()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        ///     Inicjalizacja słowników typami wiadomościami unikalnymi dla CS.
        /// </summary>
        protected override void Initialize()
        {
            //Inicjalizacja
            base.Initialize();
            //Register
            SchemaTypes.Add(Register, new Tuple<string, Type>(Resources.Register, typeof (Register)));
            //Status
            SchemaTypes.Add(Status, new Tuple<string, Type>(Resources.Status, typeof (Status)));
            //SolveRequest
            SchemaTypes.Add(SolveRequest, new Tuple<string, Type>(Resources.SolveRequest, typeof (SolveRequest)));
            //PartialProblems
            SchemaTypes.Add(PartialProblems,
                new Tuple<string, Type>(Resources.PartialProblems, typeof (SolvePartialProblems)));
            //SolutionRequest
            SchemaTypes.Add(SolutionRequest,
                new Tuple<string, Type>(Resources.SolutionRequest, typeof (SolutionRequest)));
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
            foreach (var CI in CommunicationInfos)
            {
                var listener = new Thread(StartListening);
                listener.IsBackground = true;
                listener.Start(CI);
            }
        }

        /// <summary>
        ///     Override metody komponentu rozpoczynająca nasłuchiwania serwera
        /// </summary>
        public override void Start()
        {
            InitializeMessageQueue(Info.CommunicationServerPort);
        }

        #region Constants

        private const int TimeoutModifier = 3000;

        private const string Register = "Register",
            Status = "Status",
            SolveRequest = "SolveRequest",
            PartialProblems = "PartialProblems",
            SolutionRequest = "SolutionRequest";

        #endregion

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
            ulong i = 0;
            foreach (var thread in from key in _threadStates.Keys
                where
                    _componentTypes[key] == SystemComponentType.ComputationalNode &&
                    _solvableProblemTypes[key].Contains(problemType)
                from thread in _threadStates[key]
                where thread.State == StatusThreadState.Idle
                select thread)
            {
                i++;
            }
            return i;
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
            switch (key)
            {
                case Register:
                    MsgHandler_Register((Register) message, socket);
                    return;
                case Status:
                    MsgHandler_Status((Status) message, socket);
                    return;
                case SolveRequest:
                    MsgHandler_SolveRequest((SolveRequest) message, socket);
                    return;
                case PartialProblems:
                    MsgHandler_PartialProblems((SolvePartialProblems) message, socket);
                    return;
                case SolutionRequest:
                    MsgHandler_SolutionRequest((SolutionRequest) message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
        }

        /// <summary>
        ///     Obsługa otrzymanego żądania rozwiązania
        /// </summary>
        /// <param name="solutionRequest"></param>
        /// <param name="socket"></param>
        private void MsgHandler_SolutionRequest(SolutionRequest solutionRequest, Socket socket)
        {
            //TODO: send solutionMessage
            var p = new Problem();
            throw new NotImplementedException();
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

            var response = new SolveRequestResponse {Id = problemId};
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
            if (_isPrimary && message.ErrorType == ErrorErrorType.ExceptionOccured) throw new NotImplementedException();
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
            Console.WriteLine("Status Message id={0}, sending NoOperation", status.Id);
            if (!_idToSocket.ContainsKey(status.Id))
            {
                var err = new Error {ErrorType = ErrorErrorType.UnknownSender};
                SendMessageToComponent(sender, err);
                return;
            }
            _timerStoppers[status.Id] = true;
            _threadStates[status.Id] = status.Threads;
            SendMessageToComponent(status.Id, GenerateNoOperationMessage());
        }

        /// <summary>
        ///     Metoda obsługująca otrzymaną Register message.
        /// </summary>
        /// <param name="register">Otrzymana wiadomość.</param>
        /// <param name="socket">Socket na którym ją otrzymaliśmy.</param>
        protected void MsgHandler_Register(Register register, Socket socket)
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

            var id = _firstFreeComponentId++;
            Console.WriteLine("Register Message, Sending Register Response id={0}", id);
            var timer = RegisterComponent(socket, id, ParseType(register.Type), register.SolvableProblems);
            var response = new RegisterResponse
            {
                Id = id,
                Timeout = (uint) Info.Time,
                BackupCommunicationServers = new RegisterResponseBackupCommunicationServers
                {
                    BackupCommunicationServer =
                        new RegisterResponseBackupCommunicationServersBackupCommunicationServer()
                }
            };
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
            timer.Enabled = true;
            timer.Start();
            timer.AutoReset = true;
        }

        protected Timer RegisterComponent(Socket socket, ulong id, SystemComponentType type, string[] solvableProbs)
        {
            _idToSocket.Add(id, socket);
            _socketToId.Add(socket, id);
            _componentTypes.Add(id, type);
            _threadStates.Add(id, null);
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
            timer.Interval = TimeoutModifier*(uint) Info.Time;
            _timers.Add(id, timer);
            _componentTypes.Add(id, type);
            return timer;
        }

        /// <summary>
        ///     Metoda zamykający juz nie uzywany soket
        /// </summary>
        /// <param name="socket">soket, który nalezy zamknąć</param>
        protected void CloseSocket(object socket)
        {
            Thread.Sleep(new TimeSpan(0, 0, 15, 0));
            if (socket is Socket) (socket as Socket).Close();
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
        /// <param name="Id">ID komponentu do derejestracji.</param>
        /// <returns></returns>
        protected bool Deregister(ulong Id)
        {
            //return false;
            if (!_idToSocket.ContainsKey(Id)) return false;

            var SocketCloser = new Thread(CloseSocket);
            SocketCloser.IsBackground = true;
            SocketCloser.Start(_idToSocket[Id]);

            _socketToId.Remove(_idToSocket[Id]);
            _idToSocket.Remove(Id);
            _componentTypes.Remove(Id);
            _threadStates.Remove(Id);
            _solvableProblemTypes.Remove(Id);

            _timers[Id].Enabled = false;
            _timers[Id].Close();
            _timers.Remove(Id);
            _timerStoppers.Remove(Id);
            _componentTypes.Remove(Id);
            Console.WriteLine("Deregistering id={0}", Id);
            return true;
        }

        /// <summary>
        ///     Metoda generująca nooperation message do odesłania do komponentu.
        /// </summary>
        /// <returns>NoOperation message zawierający dane o backupach.</returns>
        private NoOperation GenerateNoOperationMessage()
        {
            var noop = new NoOperation
            {
                BackupCommunicationServers = new NoOperationBackupCommunicationServers
                {
                    BackupCommunicationServer = BackupServer
                }
            };
            return noop;
        }

        #endregion

        #region ConnectionHandling

        /// <summary>
        ///     Metoda wysyłająca wiadomość do komponentu.
        /// </summary>
        /// <param name="Id">Komponent do którego wysyłamy wiadomość.</param>
        /// <param name="message">Wiadomość do wysłania.</param>
        protected void SendMessageToComponent(ulong Id, Message message)
        {
            SendMessageToComponent(_idToSocket[Id], message);
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
        public void InitializeIPList()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var local = new CommunicationInfo
            {
                CommunicationServerAddress = new Uri("http://127.0.0.1/"),
                CommunicationServerPort = Info.CommunicationServerPort
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
                    CommunicationServerPort = Info.CommunicationServerPort,
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
            var CommInfo = (CommunicationInfo) communicationInfo;
            var bytes = new byte[1024];
            var ipAddress = IPAddress.Parse(CommInfo.CommunicationServerAddress.Host);
            var tcpListener = new TcpListener(ipAddress, CommInfo.CommunicationServerPort);
            var localEndPoint = new IPEndPoint(ipAddress, CommInfo.CommunicationServerPort);
            try
            {
                tcpListener.Start();
                while (IsWorking)
                {
                    Console.WriteLine("Started listening on: {0}", CommInfo.CommunicationServerAddress);
                    var socket = tcpListener.AcceptSocket();
                    Console.WriteLine("Accepted connection from {0}", socket.RemoteEndPoint);
                    var thread = new Thread(ReceiveMessage) {IsBackground = true};
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
            ReceiveMessage((Socket) socket);
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
                    var byteArray = new byte[1024];

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
                var handler = (Socket) ar.AsyncState;
                var bytesSent = handler.EndSend(ar);
                // na potrzeby testów
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}