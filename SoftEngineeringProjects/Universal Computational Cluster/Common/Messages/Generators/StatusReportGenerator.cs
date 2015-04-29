using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages.Generators
{
    public class StatusReportGenerator
    {
        
        /// <summary>
        /// Metoda generująca message typu Status
        /// TODO: Przenieść to z powrotem, bo musi mieć dostęp do danych z wewnątrz komponentu (patrz: schema)
        /// </summary>
        /// <param name="ComponentId"></param>
        /// <returns>Status - Status Report komponentu</returns>
        public static Status Generate(ulong ComponentId,ComputationalThread[] Threads)
        {
            if (ComponentId < 0) throw new InvalidIdException();
            var result = new Status();
            result.Id = ComponentId;
            var threads = new List<StatusThread>();
            foreach (var thread in Threads) threads.Add(Generate(thread));
             result.Threads = threads.ToArray();
            return result;
        }
        public static StatusThread Generate(ComputationalThread cT)
        {
            StatusThread ST = new StatusThread();
            ST.HowLongSpecified = cT.StateChangeSpecified;
            ST.HowLong = ST.HowLongSpecified?(ulong)(10000 * (DateTime.Now.Ticks - cT.StateChange.Ticks)):0;
            ST.ProblemInstanceIdSpecified = cT.ProblemInstanceIDSpecified;
            ST.ProblemInstanceId = cT.ProblemInstanceID;
            ST.ProblemType = cT.ProblemType;
            ST.TaskIdSpecified = cT.TaskIDSpecified;
            ST.TaskId = cT.TaskID;
            ST.State = cT.State;
            return ST;
        }
    }
}
