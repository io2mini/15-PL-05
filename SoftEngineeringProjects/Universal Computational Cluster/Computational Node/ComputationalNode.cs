using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Common.Exceptions;
using Common.Messages;
using Common.Properties;
using UCCTaskSolver;

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
            ThreadStateChanged += ThreadStateChangedHandler;
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

        private void ThreadStateChangedHandler(object sender, ThreadStateChangedEventArgs e)
        {

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
                }
            }
            catch (NotEnoughIdleThreadsException e)
            {
                SendErrorMessage(e.ToString(), ErrorErrorType.ExceptionOccured);
            }
        }

        public void MsgHandler_SolvePartialProblems(SolvePartialProblems partialProblems)
        {
            var list =
                ThreadInfo.Threads.FindAll(
                    ct => (ct.ProblemType == partialProblems.ProblemType && ct.State == StatusThreadState.Idle));
            if (list.Count < partialProblems.PartialProblems.Length)
            {
                throw new NotEnoughIdleThreadsException("Not enough idle threads for problem type");
            }
            for (var i = 0; i < partialProblems.PartialProblems.Length; i++)
            {
                if (partialProblems.PartialProblems[i].NodeID == Id)
                {
                    throw new InvalidIdException("Ivalid Node Id in Partial Problem");
                }
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