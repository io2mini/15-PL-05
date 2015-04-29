using Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Klasa zawierająca parametry wątku obliczeniowego posiadanego przez Node lub TM
    /// </summary>
    public class ComputationalThread
    {
        public ulong ProblemInstanceID;
        public StatusThreadState State;
        public ulong TaskID;
        public bool ProblemInstanceIDSpecified;
        public bool TaskIDSpecified;
        public string ProblemType;
        public DateTime StateChange;
        public UCCTaskSolver.TaskSolver taskSolver;
        public bool StateChangeSpecified;
        public ComputationalThread()
        {
            ProblemInstanceIDSpecified = false;
            ProblemInstanceID = 0;
            TaskIDSpecified = false;
            TaskID = 0;
            ProblemType = null;
            StateChange = DateTime.Now;
            StateChangeSpecified = false;
            taskSolver = null;
            State = StatusThreadState.Idle;

        }
    }
}
