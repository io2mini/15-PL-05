using System;

namespace Common.Messages.Generators
{
    public static class SolveRequestGenerator
    {
        public static SolveRequest Generate(ulong? solvingTimeout = null, ulong? problemInstanceId = null)
        {
            // SYLWIA -> WARA OD MOICH KOMENTARZY (z całuskami Kuba :*).
            // Utwórz instancję.
            SolveRequest msg = new SolveRequest();

            // Uzupełnij pola

            // Pola opcjonalne
            if (solvingTimeout != null)
                msg.SolvingTimeout = (ulong) solvingTimeout;
            if (problemInstanceId != null)
                msg.Id = (ulong) problemInstanceId;

            return msg;
        }
    }
}