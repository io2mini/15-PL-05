using Common.Messages;
using System;

namespace Common.Components
{
    public class CommunicationServer : SystemComponent
    {
        public CommunicationServer() : base() { }
        protected override void HandleMessage(Messages.Message message, string key)
        {
            switch (key)
            {
                case "Register":
                    RegisterHandler((Register)message);
                    return;
                case "Status":
                    StatusHandler((Status)message);
                    return;
                default:
                    base.HandleMessage(message, key);
                    return;
            }
        }

        private void StatusHandler(Status status)
        {
            throw new NotImplementedException();
        }

        private void RegisterHandler(Register register)
        {
            throw new NotImplementedException();
        }
    }
}
