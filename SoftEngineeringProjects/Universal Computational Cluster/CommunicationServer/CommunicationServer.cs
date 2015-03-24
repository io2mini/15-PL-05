using Common.Messages;
using System;
using Common.Properties;
namespace Common.Components
{
    public class CommunicationServer : SystemComponent
    {
        const String Register = "Register", Status = "Status";
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
        public CommunicationServer() : base() { }
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
