using Common.Messages;
using Common.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DVRP;

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
            /*
             * TODO:
             * 0. If not Idle throw exception and send error
             * 1. Handle multiple threads if this TM has more than one available
             */

            var ts = GetTaskSolver(divideProblem.ProblemType, divideProblem.Data);
            var dataParts = ts.DivideProblem((int)divideProblem.ComputationalNodes);
            ulong freeTaskId = 0;

            var parts = new SolvePartialProblems();
            parts.Id = divideProblem.Id;
            parts.ProblemType = divideProblem.ProblemType;
            parts.CommonData = dataParts[0];
            //TODO: solving timeout?
            var l = new List<SolvePartialProblemsPartialProblem>((int)divideProblem.ComputationalNodes);
            for (int i = 1; i < dataParts.Count(); i++)
            {
                var spp = new SolvePartialProblemsPartialProblem();
                spp.Data = dataParts[i];
                spp.TaskId = freeTaskId++;
                spp.NodeID = this.Id;
                l.Add(spp);
            }
            parts.PartialProblems = l.ToArray();
            //TODO: implement state changes
            SendMessage(parts);
        }

        private TaskSolver GetTaskSolver(string problemType, byte[] data)
        {
            /* TODO:
             * 1. Initialize apropriate task solver based on problem type
             * 2. If TM doesn't implement solving given problem type: throw exception
             */
            throw new NotImplementedException();
        }
    }
}
