using System;

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
    }
}
