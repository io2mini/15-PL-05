using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Common.Messages;
using Common.Properties;
using DVRP;

namespace Common.Components
{
    public class TaskManager : SolvingComponent
    {
        private const string DivideProblem = "DivideProblem", Solutions = "Solutions";

        public TaskManager()
        {
            DeviceType = SystemComponentType.TaskManager;
            SolvableProblems = new[] { "DVRP" };
            PararellThreads = 1;
        }

        protected override void Initialize()
        {
            base.Initialize();
            //DivideProblem
            SchemaTypes.Add(DivideProblem, new Tuple<string, Type>(Resources.DivideProblem, typeof(DivideProblem)));
            //SolutionRequest
            SchemaTypes.Add(Solutions, new Tuple<string, Type>(Resources.Solution, typeof(Solutions)));
        }

        protected override void HandleMessage(Message message, string key, Socket socket)
        {
            switch (key)
            {
                case DivideProblem:
                    MsgHandler_DivideProblem((DivideProblem)message, socket);
                    return;
                case Solutions:
                    MsgHandler_Solutions((Solutions)message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
        }

        private void MsgHandler_Solutions(Messages.Solutions solutions, Socket socket)
        {
            var ts = GetTaskSolver(solutions.ProblemType, solutions.CommonData);
            var byteList = new List<byte[]>();
            foreach (var S in solutions.Solutions1)
            {
                byteList.Add(S.Data);
            }
            byte[] solution = ts.MergeSolution(byteList.ToArray());
            SolutionsSolution s = new SolutionsSolution();
            s.Data = solution;
            s.ComputationsTime = (ulong)solutions.Solutions1.Max<SolutionsSolution>((d)=>((decimal)d.ComputationsTime));
           
            s.TaskId = ulong.MaxValue;
             s.TaskIdSpecified = false;
            s.TimeoutOccured = false;
            s.Type = SolutionsSolutionType.Final;

            SendMessage(Common.Messages.Generators.SolutionGenerator.Generate(solutions.CommonData, solutions.Id, solutions.ProblemType, new[] { s }));

        }

        private void MsgHandler_DivideProblem(DivideProblem divideProblem, Socket socket)
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
            parts.CommonData = dataParts[0]; //TODO: check for null?
            //TODO: solving timeout?
            var l = new List<SolvePartialProblemsPartialProblem>((int)divideProblem.ComputationalNodes);

            for (var i = 1; i < dataParts.Count(); i++)
            {
                var spp = new SolvePartialProblemsPartialProblem
                {
                    Data = dataParts[i],
                    TaskId = freeTaskId++,
                    NodeID = Id
                };
                l.Add(spp);
            }
            parts.PartialProblems = l.ToArray();
            //TODO: implement state changes
            SendMessage(parts);
        }


    }
}