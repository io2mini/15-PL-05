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
            // TODO: Konwertuj do danych umozliwiajacych obliczenia
            // TODO: Kod algorytmu idzie tutaj
            // TODO: Wyslij obliczony problem
        }
    }
}
