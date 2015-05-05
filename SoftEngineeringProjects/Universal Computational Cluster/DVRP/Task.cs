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
        public Task(Problem problem, Route[][] routes)
        {
            Problem = problem;
            Routes = routes;
        }

        /// <summary>
        ///     Dane problemu
        /// </summary>
        public Problem Problem { get; private set; }

        /// <summary>
        ///     Sekwencje do sprawdzenia
        /// </summary>
        public Route[][] Routes { get; private set; }

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