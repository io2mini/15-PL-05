using System;
using Common.Messages;

namespace Common
{
    public class ThreadStateChangedEventArgs : EventArgs
    {
        public ThreadStateChangedEventArgs(ulong problemInstanceId,bool problemInstanceIdSpecified, ulong taskId,bool taskIdSpecified, StatusThreadState oldState, StatusThreadState newState)
        {
            ProblemInstanceId = problemInstanceIdSpecified?(ulong?)null:problemInstanceId;
            TaskId = taskIdSpecified?(ulong?)null:taskId;
            OldState = oldState;
            NewState = newState;
        }

        public ulong? ProblemInstanceId;
        public ulong? TaskId;
        public StatusThreadState OldState;
        public StatusThreadState NewState;
        
    }
    public delegate void ThreadStateChanged(object sender, EventArgs e); //TODO: create our event args class
}
