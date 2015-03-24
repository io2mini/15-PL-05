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
        const String Register = "Register", Status = "Status";
        Dictionary<ulong, bool> TimerStoppers;
        Dictionary<ulong, Timer> Timers;
        Dictionary<ulong, Socket> Sockets;
        ulong FirstFreeID;

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

        public CommunicationServer() : base()
        {
            FirstFreeID = 0;
            TimerStoppers = new Dictionary<ulong, bool>();
            Sockets = new Dictionary<ulong, Socket>();
        }

        protected override void HandleMessage(Messages.Message message, string key,Socket socket)
        {
            switch (key)
            {
                case Register:
                    RegisterHandler((Register)message,socket);
                    return;
                case Status:
                    StatusHandler((Status)message);
                    return;
                default:
                    base.HandleMessage(message, key,socket);
                    return;
            }
        }

        private NoOperation GenerateNoOperationMessage()
        {
            NoOperation Noop = new NoOperation();
            Noop.BackupCommunicationServers = new NoOperationBackupCommunicationServers();
            Noop.BackupCommunicationServers.BackupCommunicationServer = BackupServer;
            return Noop;
        }

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

        protected void SendMessageToComponent(Socket socket, Message m)
        {
            try
            {
                byte[] byteData = Encoding.ASCII.GetBytes(m.toString());
                socket.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), socket);
            }
            catch (Exception e)
            {
                String message = "Unable to send message";
                throw new MessageNotSentException(message, e);
            }
        }

        private void StatusHandler(Status status)
        {
            TimerStoppers[status.Id] = true;
            SendMessageToComponent(Sockets[status.Id], GenerateNoOperationMessage());
        }

        private bool Deregister(ulong Id)
        {
            if (!Sockets.ContainsKey(Id)) return false;
            Sockets.Remove(Id);
            TimerStoppers.Remove(Id);
            Timers.Remove(Id);
            return true;
        }

        private void RegisterHandler(Register register,Socket socket)
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
            Timers.Add(id,new Timer((u) => {
                if (!TimerStoppers[(ulong)u]) TimerStoppers.Remove((ulong)u);
                else TimerStoppers[(ulong)u] = false;
            }, id, 0, (int)CommunicationInfo.Time));
            RegisterResponse response = new RegisterResponse();
            response.Id = id;
            response.Timeout = (uint)CommunicationInfo.Time;
            response.BackupCommunicationServers = new RegisterResponseBackupCommunicationServers();
            response.BackupCommunicationServers.BackupCommunicationServer =
                new RegisterResponseBackupCommunicationServersBackupCommunicationServer();
            response.BackupCommunicationServers.BackupCommunicationServer.address =
                BackupServer.address;
            response.BackupCommunicationServers.BackupCommunicationServer.port =
                BackupServer.port;
            response.BackupCommunicationServers.BackupCommunicationServer.portSpecified =
                BackupServer.portSpecified;
            SendMessageToComponent(socket, response);
        }
    }
}
