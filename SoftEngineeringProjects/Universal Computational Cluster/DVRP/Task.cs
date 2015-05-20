using System;
using DVRP.Objects;

namespace DVRP
{
    [Serializable]
    public class Task
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="problem">Problem do rozwiązania</param>
        /// <param name="routes">Tablica tras do sprawdzenia</param>
        public Task(int[][] brackets)
        {
            Brackets = brackets;
        }

        /// <summary>
        ///     Sekwencje do sprawdzenia
        /// </summary>
        public int[][] Brackets { get; private set; }

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