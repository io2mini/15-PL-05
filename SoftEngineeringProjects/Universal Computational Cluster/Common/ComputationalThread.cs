using System;
using Common.Messages;
using UCCTaskSolver;

namespace Common
{
    /// <summary>
    ///     Klasa zawierająca parametry wątku obliczeniowego posiadanego przez Node lub TM
    /// </summary>
    public class ComputationalThread
    {
        public ulong ProblemInstanceId;
        public bool ProblemInstanceIdSpecified;
        public string ProblemType;
        public StatusThreadState State;
        public DateTime StateChange;
        public bool StateChangeSpecified;
        public ulong TaskId;
        public bool TaskIdSpecified;
        public TaskSolver TaskSolver;

        public ComputationalThread()
        {
            ProblemInstanceIdSpecified = false;
            ProblemInstanceId = 0;
            TaskIdSpecified = false;
            TaskId = 0;
            ProblemType = null;
            StateChange = DateTime.Now;
            StateChangeSpecified = false;
            TaskSolver = null;
            State = StatusThreadState.Idle;
        }
    }
}