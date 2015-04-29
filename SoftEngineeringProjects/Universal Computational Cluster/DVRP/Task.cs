using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVRP
{
    [Serializable]
    public class Task
    {
        /// <summary>
        /// Dane problemu
        /// </summary>
        public Problem Problem { get; private set; }

        /// <summary>
        /// Sekwencje do sprawdzenia
        /// </summary>
        public uint[][] Sequences { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p">Dane problemu</param>
        /// <param name="sequences">Sekwencje do sprawdzenia</param>
        public Task(Problem p, uint[][] sequences)
        {
            Problem = p;
            Sequences = sequences;
        }

        public static Task Deserialize(byte[] byteArray)
        {
            return (Task)Serializer.Deserialize(byteArray);
        }

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }
    }
}
