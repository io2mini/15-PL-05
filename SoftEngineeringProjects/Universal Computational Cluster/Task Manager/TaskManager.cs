using Common.Messages;
using Common.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Common.Components
{
    public class TaskManager : SystemComponent
    {
        const String DivideProblem = "DivideProblem";
        public TaskManager() : base() 
        {
            deviceType = SystemComponentType.TaskManager;
            solvableProblems = new string[] { "DVRP" };
            pararellThreads = 1;
        }

        protected override void Initialize()
        {
            base.Initialize();
            //DivideProblem
            SchemaTypes.Add(DivideProblem, new Tuple<string, Type>(Resources.Error, typeof(DivideProblem)));
        }

        protected override void HandleMessage(Message message, string key, Socket socket)
        {
            switch (key)
            {
                case DivideProblem:
                    MsgHandler_DivideProblem((DivideProblem)message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
        }

        private void MsgHandler_DivideProblem(Messages.DivideProblem divideProblem, Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}
