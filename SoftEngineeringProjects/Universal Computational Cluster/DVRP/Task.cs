using System;

namespace DVRP
{
    [Serializable]
    public class Task
    {
        /// <summary>
        /// </summary>
        /// <param name="p">Dane problemu</param>
        /// <param name="sequences">Sekwencje do sprawdzenia</param>
        public Task(Problem p, uint[][] sequences)
        {
            Problem = p;
            Sequences = sequences;
        }

        /// <summary>
        ///     Dane problemu
        /// </summary>
        public Problem Problem { get; private set; }

        /// <summary>
        ///     Sekwencje do sprawdzenia
        /// </summary>
        public uint[][] Sequences { get; private set; }

        public static Task Deserialize(byte[] byteArray)
        {
            return (Task) Serializer.Deserialize(byteArray);
        }

        public byte[] Serialize()
        {
            return Serializer.Serialize(this);
        }
    }
}