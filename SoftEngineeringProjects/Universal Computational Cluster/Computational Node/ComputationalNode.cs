using Common.Exceptions;
using Common.Messages;
using System;
using UCCTaskSolver;

namespace Common.Components
{
    public class ComputationalNode : SystemComponent
    {
        const string SolvePartialProblems = "SolvePartialProblems";
        public ComputationalNode()
            : base()
        {
            deviceType = SystemComponentType.ComputationalNode;
            solvableProblems = new string[] { "DVRP" };
            pararellThreads = 1;
        }
        protected override void HandleMessage(Message message, string key, System.Net.Sockets.Socket socket)
        {
            try
            {
                switch (key)
                {
                    case SolvePartialProblems:
                        MsgHandler_SolvePartialProblems((SolvePartialProblems)message);
                        break;
                }
            }
            catch (NotEnoughIdleThreadsException e)
            {
                SendExceptionErrorMessage(e);
            }
        }
        public void MsgHandler_SolvePartialProblems(Messages.SolvePartialProblems partialProblems)
        {
            var list = threadInfo.Threads.FindAll((CT) => (CT.ProblemType == partialProblems.ProblemType && CT.State == StatusThreadState.Idle));
            if (list.Count < partialProblems.PartialProblems.Length)
            {
                throw new NotEnoughIdleThreadsException("Not enough idle threads for problem type");
            }
            for (int i = 0; i < partialProblems.PartialProblems.Length; i++)
            {
                if (partialProblems.PartialProblems[i].NodeID == Id)
                {
                    //TODO: Send Error Message
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
