using System;
using UCCTaskSolver;

namespace Common.Components
{
    public class ComputationalNode : SystemComponent
    {
        public ComputationalNode() : base()
        {
            deviceType = SystemComponentType.ComputationalNode;
            solvableProblems = new string[] { "DVRP" };
            pararellThreads = 1;
        }

        public void MsgHandler_SolvePartialProblems(Messages.SolvePartialProblems partialProblems)
        {
            // TODO: if not idle throw jakiś exception i wyślij error msg
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
