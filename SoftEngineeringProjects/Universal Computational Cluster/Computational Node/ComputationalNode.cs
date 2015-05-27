using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using Common.Exceptions;
using Common.Messages;
using Common.Messages.Generators;
using Common.Properties;
using DVRP;
using TaskSolver = UCCTaskSolver.TaskSolver;

// ReSharper disable once CheckNamespace
namespace Common.Components
{

    public class ComputationalNode : SolvingComponent
    {
        private const string SolvePartialProblems = "SolvePartialProblems", SolutionRequest = "SolutionRequest";
        
        public ComputationalNode()
        {
            DeviceType = SystemComponentType.ComputationalNode;
            SolvableProblems = new[] {"DVRP"};
            PararellThreads = 1; //TODO: load this from config
           
        }

        protected override void Initialize()
        {
            base.Initialize();
            //SolutionRequest
            SchemaTypes.Add(SolutionRequest, new Tuple<string, Type>(Resources.SolutionRequest, typeof(SolutionRequest)));
            //SolvePartialProblems
            SchemaTypes.Add(SolvePartialProblems, new Tuple<string, Type>(Resources.PartialProblems, typeof(SolvePartialProblems)));
        }

        protected override void HandleMessage(Message message, string key, Socket socket)
        {
            try
            {
                switch (key)
                {
                    case SolvePartialProblems:
                        MsgHandler_SolvePartialProblems((SolvePartialProblems) message);
                        break;
                    case SolutionRequest:
                        MsgHandler_SolutionRequest((SolutionRequest)message);
                        break;
                    default:
                        base.HandleMessage(message, key, socket);
                        return;
                }
            }
            catch (NotEnoughIdleThreadsException e)
            {
                SendErrorMessage(e.ToString(), ErrorErrorType.ExceptionOccured);
            }
        }

        private void MsgHandler_SolutionRequest(global::SolutionRequest solutionRequest)
        {
            // TODO: Przenieść to do innego miejsca
            if (ThreadInfo.Threads.All(t => t.SolutionData != null))
            {
                int index = 0;
                double minCost = Double.MaxValue;

                // Wszyscy gotowi - odbierz dane
                for (int i = 0; i < ThreadInfo.Threads.Count; i++)
                {
                    Solution s = (DVRP.Solution)DVRP.Solution.Deserialize(ThreadInfo.Threads[i].SolutionData);
                    if (s.Cost < minCost)
                    {
                        index = i;
                        minCost = s.Cost;
                    }
                }

                // TODO: Jak dla mnie to to nie bedzie dobrze działać

                var solution = SolutionGenerator.Generate(ThreadInfo.Threads[index].CommonData,
                    ThreadInfo.Threads[index].ProblemInstanceId, ThreadInfo.Threads[index].ProblemType,
                    new SolutionsSolution[] { new SolutionsSolution() { Data = ThreadInfo.Threads[index].SolutionData, Type = SolutionsSolutionType.Final } });

            }
            else
            {
                var solution = SolutionGenerator.Generate(null,
                    ThreadInfo.Threads[0].ProblemInstanceId, ThreadInfo.Threads[0].ProblemType,
                    new SolutionsSolution[] { new SolutionsSolution() { Type = SolutionsSolutionType.Final } });
            }
        }

        public void MsgHandler_SolvePartialProblems(SolvePartialProblems partialProblems)
        {
            var list =
                ThreadInfo.Threads.FindAll(
                    ct => (ct.State == StatusThreadState.Idle));
            if (list.Count < partialProblems.PartialProblems.Length-1)
            {
                throw new NotEnoughIdleThreadsException("Not enough idle threads for problem type");
            }
            if (partialProblems.PartialProblems.Any(t => t.NodeID != Id))
            {
                throw new InvalidIdException("Ivalid Node Id in Partial Problem");
            }
            for(int i=0;i<partialProblems.PartialProblems.Length;i++)
            {
                list[i].TaskSolver = GetTaskSolver(partialProblems.ProblemType,
                    partialProblems.PartialProblems[0].Data);
                list[i].StartSolving(partialProblems.Id,partialProblems.ProblemType,partialProblems.PartialProblems[i+1].TaskId,new TimeSpan(0,0,0,0,(int)partialProblems.SolvingTimeout),partialProblems.PartialProblems[0].Data,partialProblems.PartialProblems[i
                    +1].Data);
            }
            // TODO: implement state changes for threads
            // TODO: implement solving threads
            // TODO: save solutions
        }

        
    }
}