﻿using System;
using System.Threading;
using Common.Configuration;
using Common.Messages;
using UCCTaskSolver;

namespace Common
{
    /// <summary>
    ///     Klasa zawierająca parametry wątku obliczeniowego posiadanego przez Node lub TM
    /// </summary>
    public delegate void SolutionCallback(ComputationalThread ct);
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
        public Thread Solver;
        public ThreadInfo Localisation;
        private Mutex WaitForSolution;
        private byte[] solutionData;

        public byte[] SolutionData
        {
            get
            {
                if (WaitForSolution != null) WaitForSolution.WaitOne();
                byte[] sd = solutionData;
                if (WaitForSolution != null) WaitForSolution.ReleaseMutex();
                return sd;
            }
            set
            {
                if (WaitForSolution != null) WaitForSolution.WaitOne();
                solutionData = value;
                if (WaitForSolution != null) WaitForSolution.ReleaseMutex();
            }
        }
        public byte[] CommonData;
    
        public void StartSolving(ulong problemInstanceId, string problemType, ulong taskId, TimeSpan timeout, byte[] commonData, byte[] data,SolutionCallback solutionCallback)
        {
            ProblemInstanceId = problemInstanceId;
            ProblemInstanceIdSpecified = true;
            ProblemType = problemType;
            WaitForSolution = new Mutex();
            TaskId = taskId;
            TaskIdSpecified = true;
            State = StatusThreadState.Busy;
            CommonData = commonData;
            SolutionData = null;
            ThreadStart starter = () => Solve(data, timeout,solutionCallback);
            Solver = new Thread(starter);
            Solver.Start();
        }
        private void Solve(byte[] data, TimeSpan timeout,SolutionCallback s)
        {
            SolutionCallback(TaskSolver.Solve(data, timeout),s);
        }
        private void SolutionCallback(byte[] data,SolutionCallback s)
        {
            SolutionData = data;
            s(this);
        }
        public ComputationalThread()
        {
            ProblemInstanceIdSpecified = false;
            ProblemInstanceId = 0;
            TaskIdSpecified = false;
            TaskId = 0;
            ProblemType = null;
            StateChange = DateTime.Now;
            StateChangeSpecified = false;
            Solver = null;
            TaskSolver = null;
            State = StatusThreadState.Idle;
            SolutionData = null;
        }

        public void SetStatus(StatusThreadState status)
        {
            // TODO: ignore callback if aborting
            if (status == StatusThreadState.Idle)
            {
                Solver.Abort();
                Solver = null;
            }
            State = status;
        }
    }
}