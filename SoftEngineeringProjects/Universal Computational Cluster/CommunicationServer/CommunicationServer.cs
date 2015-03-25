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
namespace Common.Components
{
    public class CommunicationServer : SystemComponent
    {
        const int TimeoutModifier = 3;
        const String Register = "Register", Status = "Status";
        Dictionary<ulong, bool> TimerStoppers;
        Dictionary<ulong, Timer> Timers;
        Dictionary<ulong, Socket> Sockets;
        ulong FirstFreeID;


        public CommunicationServer()
            : base()
        {
            FirstFreeID = 0;

            TimerStoppers = new Dictionary<ulong, bool>();
            Sockets = new Dictionary<ulong, Socket>();

        }
        /// <summary>
        /// Inicjalizacja słowników typami wiadomościami unikalnymi dla CS.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            DictionaryKeys.Add(Register);
            DictionaryKeys.Add(Status);
            Schemas.Add(Register, Resources.Register);
            Schemas.Add(Status, Resources.Status);
            MessageTypes.Add(Status, typeof(Status));
            MessageTypes.Add(Register, typeof(Register));
        }

        /// <summary>
        /// Metoda rozpoczynająca nasłuchiwania asynchroniczne na wiadomości.
        /// </summary>
        public override void Start()
        {
            Thread thread = new Thread(StartListening);
            thread.IsBackground = true;
            thread.Start();
        }
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
                    RegisterHandler((Register)message, socket);
                    return;
                case Status:
                    StatusHandler((Status)message);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
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
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
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
        /// <summary>
        /// Metoda obsługująca otrzymaną wiadomość typu status.
        /// </summary>
        /// <param name="status">Otrzymany status.</param>
        private void StatusHandler(Status status)
        {
            TimerStoppers[status.Id] = true;
            SendMessageToComponent(status.Id, GenerateNoOperationMessage());
        }
        /// <summary>
        /// Metoda usuwająca komponent i zrywająca z nim połączenie.
        /// </summary>
        /// <param name="Id">ID komponentu do derejestracji.</param>
        /// <returns></returns>
        private bool Deregister(ulong Id)
        {
            if (!Sockets.ContainsKey(Id)) return false;
            Sockets[Id].Close();
            Sockets.Remove(Id);
            TimerStoppers.Remove(Id);
            Timers.Remove(Id);
            return true;
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
                Validate(message, socket);
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
        /// Metoda obsługująca otrzymaną Register message.
        /// </summary>
        /// <param name="register">Otrzymana wiadomość.</param>
        /// <param name="socket">Socket na którym ją otrzymaliśmy.</param>
        private void RegisterHandler(Register register, Socket socket)
        {
            if (register.DeregisterSpecified)
            {
                if (register.Deregister)
                {
                    Deregister(register.Id);
                    return;
                }
            }

            ulong id = FirstFreeID++;
            Sockets.Add(id, socket);
            TimerStoppers.Add(id, true);
            Timers.Add(id, new Timer((u) =>
            {
                if (!TimerStoppers[(ulong)u]) TimerStoppers.Remove((ulong)u);
                else TimerStoppers[(ulong)u] = false;
            }, id, 0, (int)CommunicationInfo.Time));
            RegisterResponse response = new RegisterResponse();
            response.Id = id;
            response.Timeout = TimeoutModifier * (uint)CommunicationInfo.Time;
            response.BackupCommunicationServers = new RegisterResponseBackupCommunicationServers();
            response.BackupCommunicationServers.BackupCommunicationServer =
                new RegisterResponseBackupCommunicationServersBackupCommunicationServer();
            response.BackupCommunicationServers.BackupCommunicationServer.address =
                BackupServer.address;
            response.BackupCommunicationServers.BackupCommunicationServer.port =
                BackupServer.port;
            response.BackupCommunicationServers.BackupCommunicationServer.portSpecified =
                BackupServer.portSpecified;
            SendMessageToComponent(id, response);
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
    }
}
