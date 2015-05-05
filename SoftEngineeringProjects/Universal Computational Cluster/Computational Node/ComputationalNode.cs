using System;
using System.Net.Sockets;
using Common.Exceptions;
using Common.Messages;
using UCCTaskSolver;
using System.Collections.Generic;

namespace Common.Components
{
    public class ComputationalNode : SystemComponent
    {
        private const string SolvePartialProblems = "SolvePartialProblems";
      
        public ComputationalNode()
        {
            DeviceType = SystemComponentType.ComputationalNode;
            SolvableProblems = new[] {"DVRP"};
            PararellThreads = 1;
           
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
                    CT => (CT.ProblemType == partialProblems.ProblemType && CT.State == StatusThreadState.Idle));
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

            // TODO: jesli mamy mniej idle threadow niz powinnismy rzucamy error
            // TODO: jeśli któryś problem ma różniący się NodeId niż ten CN to send apropriate error msg;
            // TODO: implement state changes for threads
            // TODO: implement solving threads
            // TODO: save solutions
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