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
        public static Status Generate(ulong ComponentId)
        {
            if (ComponentId < 0) throw new NegativeIdException();
            var result = new Status();
            result.Id = ComponentId;
            var threads = new List<StatusThread>();
            var statusthread = new StatusThread();
            statusthread.State = StatusThreadState.Idle;
            threads.Add(statusthread);
            result.Threads = threads.ToArray();
            return result;
        }
    }
}
