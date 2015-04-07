using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
    /// <summary>
    /// Klasa odpowiedzialna za trzymanie informacji o Communication Serverze, do którego wysyłane sa komunikaty
    /// typu RegisterMessage
    /// </summary>
    public class CommunicationInfo
    {
        public Uri CommunicationServerAddress { get; set; }
        public ushort CommunicationServerPort { get; set; }
        public ulong Time { get; set; }
        public bool IsBackup { get; set; }

    }
}
