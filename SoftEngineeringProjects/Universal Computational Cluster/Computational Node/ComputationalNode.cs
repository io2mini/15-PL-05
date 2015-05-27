using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Threading;
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
            SolvableProblems = new[] { "DVRP" };
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
                        MsgHandler_SolvePartialProblems((SolvePartialProblems)message);
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

        private void PrepareSolutions(object ProblemID)
        {
            PrepareSolutions((ulong)ProblemID);
        }
        private void PrepareSolutions(ulong ProblemID)
        {
            // TODO: Przenieść to do innego miejsca
            Solutions solution;
            foreach (var ct in ThreadInfo.Threads.Where((s) => (s.ProblemInstanceIdSpecified == true && s.ProblemInstanceId == ProblemID)))
            {
                ct.Solver.Join();
            }
            if (ThreadInfo.Threads.All(t => t.SolutionData != null))
            {
                ComputationalThread data = null;
                double minCost = Double.MaxValue;

                // Wszyscy gotowi - odbierz dane
                foreach (var ct in ThreadInfo.Threads.Where((s) => (s.ProblemInstanceIdSpecified == true && s.ProblemInstanceId == ProblemID)))
                {
                    Solution s = (DVRP.Solution)DVRP.Solution.Deserialize(ct.SolutionData);
                    if (s.Cost < minCost)
                    {
                        data = ct;
                        minCost = s.Cost;
                    }
                }

                // TODO: Jak dla mnie to to nie bedzie dobrze działać

                solution = SolutionGenerator.Generate(data.CommonData,
                    data.ProblemInstanceId, data.ProblemType,
                    new SolutionsSolution[] { new SolutionsSolution() { Data = data.SolutionData, Type = SolutionsSolutionType.Final } });

            }
            else
            {
                solution = SolutionGenerator.Generate(null,
                    ThreadInfo.Threads[0].ProblemInstanceId, ThreadInfo.Threads[0].ProblemType,
                    new SolutionsSolution[] { new SolutionsSolution() { Type = SolutionsSolutionType.Final } });
            }
            SendMessage(solution);
        }
        public void SendSolution(ComputationalThread ct)
        {
            Solution s = (DVRP.Solution)DVRP.Solution.Deserialize(ct.SolutionData);
            var solution = SolutionGenerator.Generate(ct.CommonData, ct.ProblemInstanceId, ct.ProblemType, new[] {new SolutionsSolution()
            {
                TaskId = ct.TaskId,
                TaskIdSpecified = true,
                Type = SolutionsSolutionType.Partial,
                ComputationsTime = (ulong)(DateTime.Now - ct.StateChange).Ticks,
                Data = ct.SolutionData,
                TimeoutOccured = false
            }
            }            )
            ;
            SendMessage(solution);
        }
        public void MsgHandler_SolvePartialProblems(SolvePartialProblems partialProblems)
        {
            var list =
                ThreadInfo.Threads.FindAll(
                    ct => (ct.State == StatusThreadState.Idle));
            if (list.Count < partialProblems.PartialProblems.Length - 1)
            {
                throw new NotEnoughIdleThreadsException("Not enough idle threads for problem type");
            }
            if (partialProblems.PartialProblems.Any(t => t.NodeID != Id))
            {
                throw new InvalidIdException("Ivalid Node Id in Partial Problem");
            }
            for (int i = 0; i < partialProblems.PartialProblems.Length; i++)
            {
                list[i].TaskSolver = GetTaskSolver(partialProblems.ProblemType,
                    partialProblems.CommonData);
                list[i].StartSolving(partialProblems.Id, partialProblems.ProblemType, partialProblems.PartialProblems[i].TaskId,
                    new TimeSpan(0, 0, 0, 0, (int)partialProblems.SolvingTimeout), partialProblems.CommonData, partialProblems.PartialProblems[i].Data, SendSolution);
            }

            // TODO: implement state changes for threads
            // TODO: implement solving threads
            // TODO: save solutions
        }


    }
}