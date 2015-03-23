using System;

namespace SystemComponent.Control.Helpers
{
    public class CommandLineParameters
    {
        private Uri communicationServerAddress;
        private ushort communicationServerPort;

        public CommandLineParameters(Uri communicationServerAddress, ushort communicationServerPort)
        {
            this.communicationServerAddress = communicationServerAddress;
            this.communicationServerPort = communicationServerPort;
        }

        public Uri CommunicationServerAddress
        {
            get { return communicationServerAddress; }
        }

        public ushort CommunicationServerPort
        {
            get { return communicationServerPort; }
        }
    }
}