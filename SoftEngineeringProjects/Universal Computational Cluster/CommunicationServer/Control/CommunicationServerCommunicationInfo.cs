using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Communication;
namespace CommunicationServer.Control
{
    public class CommunicationServerCommunicationInfo : CommunicationInfo
    {
        public ulong Time { get; set; }
        public bool IsBackup { get; set; }
        public uint Port { get; set; }
    }
}
