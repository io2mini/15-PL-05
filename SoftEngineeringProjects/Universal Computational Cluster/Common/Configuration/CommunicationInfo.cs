using System;

namespace Common.Configuration
{
    /// <summary>
    ///     Klasa odpowiedzialna za trzymanie informacji o Communication Serverze, do którego wysyłane sa komunikaty
    ///     typu RegisterMessage
    /// </summary>
    public class CommunicationInfo
    {
        public Uri CommunicationServerAddress { get; set; }
        public ushort CommunicationServerPort { get; set; }
        public ulong Time { get; set; }
        public bool IsBackup { get; set; }
        public int ThreadNumber { get; set; }
    }
}