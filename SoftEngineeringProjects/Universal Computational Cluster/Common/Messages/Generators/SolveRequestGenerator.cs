namespace Common.Messages.Generators
{
    public static class SolveRequestGenerator
    {
        public static SolveRequest Generate(string problemType, byte[] serializedProblem, ulong? solvingTimeout = null,
            ulong? problemInstanceId = null)
        {
            // Utwórz instancję.
            var msg = new SolveRequest
            {
                ProblemType = problemType,
                Data = serializedProblem
            };

            // Pola opcjonalne
            if (solvingTimeout != null)
                msg.SolvingTimeout = (ulong) solvingTimeout;
            if (problemInstanceId != null)
                msg.Id = (ulong) problemInstanceId;

            return msg;
        }
    }
}