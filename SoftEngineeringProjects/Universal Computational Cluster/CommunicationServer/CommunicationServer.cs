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
        ulong FreeKey;
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
        public CommunicationServer()
            : base()
        {
            FreeKey = 0;
            TimerStoppers = new Dictionary<ulong, bool>();
            Sockets = new Dictionary<ulong, Socket>();
        }
        protected override void HandleMessage(Messages.Message message, string key)
        {
            switch (key)
            {
                case Register:
                    RegisterHandler((Register)message);
                    return;
                case Status:
                    StatusHandler((Status)message);
                    return;
                default:
                    base.HandleMessage(message, key);
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
        protected void SendMessageToComponent(Socket tcpListener,Message m)
        {
            try
            {

                String message = m.GetMessage();
                
                
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
        private void RegisterHandler(Register register)
        {
            if (register.DeregisterSpecified)
            {
                if (register.Deregister)
                {
                    Deregister(register.Id);
                    return;
                }
            }

            ulong id = FreeKey++;
            TimerStoppers.Add(id, true);
            Timers.Add(id,new Timer((u) => {
                if (!TimerStoppers[(ulong)u]) TimerStoppers.Remove((ulong)u);
                else TimerStoppers[(ulong)u] = false;
            }, id, 0, (int)CommunicationInfo.Time));
        }
    }
}
