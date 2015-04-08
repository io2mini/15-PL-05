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
        /// </summary>
        /// <param name="ComponentId"></param>
        /// <returns>Status - Status Report komponentu</returns>
        public static Status Generate(ulong ComponentId)
        {
            if (ComponentId < 0) throw new NegativeIdException();
            var result = new Status();
            result.Id = ComponentId;
            return result;
        }
    }
}
