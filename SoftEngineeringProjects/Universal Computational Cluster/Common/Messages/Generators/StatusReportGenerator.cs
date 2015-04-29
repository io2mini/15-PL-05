using System;
using System.Linq;

namespace Common.Messages.Generators
{
    public class StatusReportGenerator
    {
        /// <summary>
        ///     Metoda generująca message typu Status
        ///     TODO: Przenieść to z powrotem, bo musi mieć dostęp do danych z wewnątrz komponentu (patrz: schema)
        /// </summary>
        /// <param name="componentId"></param>
        /// <param name="threads"></param>
        /// <returns>Status - Status Report komponentu</returns>
        public static Status Generate(ulong componentId, ComputationalThread[] threads)
        {
            var result = new Status
            {
                Id = componentId,
                Threads = threads.Select(Generate).ToArray()
            };
            return result;
        }

        public static StatusThread Generate(ComputationalThread cT)
        {
            var st = new StatusThread {HowLongSpecified = cT.StateChangeSpecified};
            st.HowLong = st.HowLongSpecified ? (ulong) (10000*(DateTime.Now.Ticks - cT.StateChange.Ticks)) : 0;
            st.ProblemInstanceIdSpecified = cT.ProblemInstanceIdSpecified;
            st.ProblemInstanceId = cT.ProblemInstanceId;
            st.ProblemType = cT.ProblemType;
            st.TaskIdSpecified = cT.TaskIdSpecified;
            st.TaskId = cT.TaskId;
            st.State = cT.State;
            return st;
        }
    }
}