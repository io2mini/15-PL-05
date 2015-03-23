using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemComponent.Control
{
    public class CommunicationServerParameters
    {
        public ulong Time { get; set; }
        public bool IsBackup { get; set; }
        public uint Port { get; set; }
    }
}
