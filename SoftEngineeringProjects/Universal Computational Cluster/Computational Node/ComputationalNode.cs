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

    public class ComputationalNode : SystemComponent
    {
        private const string SolvePartialProblems = "SolvePartialProblems", SolutionRequest = "SolutionRequest";
        
        public ComputationalNode()
        {
            DeviceType = SystemComponentType.ComputationalNode;
            SolvableProblems = new[] {"DVRP"};
            PararellThreads = 1; //TODO: load this from config
            
            TaskSolverFactories = new Dictionary<string, TaskSolverFactory>();
            TaskSolverFactories.Add("DVRP",DVRP.TaskSolver.TaskSolverFactory);
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

                var solution = SolutionGenerator.Generate(ThreadInfo.Threads[index].CommonData,
                    ThreadInfo.Threads[index].ProblemInstanceId, ThreadInfo.Threads[index].ProblemType,
                    new SolutionsSolution[] {new SolutionsSolution() {//TODO: Dodać
                    } });

            }
            else
            {
                
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

        public delegate TaskSolver TaskSolverFactory(byte[] data);

        private Dictionary<string, TaskSolverFactory> TaskSolverFactories;
        protected TaskSolver GetTaskSolver(string problemType, byte[] data)
        {
            /* TODO:
             * 1. Initialize apropriate task solver based on problem type
             * 2. If TM doesn't implement solving given problem type: throw exception
             */
            if (!SolvableProblems.Contains(problemType))
            {
                //TODO: exception message
                throw new UnrecognizedProblemException();
            }
            if (!TaskSolverFactories.ContainsKey(problemType))
            {
                //TODO: exception message
                throw new UnrecognizedProblemException();
            }
            return TaskSolverFactories[problemType](data);
        }
    }
}