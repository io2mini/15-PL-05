using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Exceptions;
using DVRP;

namespace Common
{
    public class SolvingComponent : SystemComponent
    {
        public SolvingComponent()
        {

            TaskSolverFactories = new Dictionary<string, TaskSolverFactory>();
            TaskSolverFactories.Add("DVRP", DVRP.TaskSolver.TaskSolverFactory);
        }
        public delegate TaskSolver TaskSolverFactory(byte[] data);

        protected Dictionary<string, TaskSolverFactory> TaskSolverFactories;
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
