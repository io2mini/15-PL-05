namespace Common.Messages.Generators
{
    public static class RegisterGenerator
    {
        public static Register Generate(SystemComponentType deviceType, string[] solvableProblems, byte parallelThreads,
            bool deregister = false, ulong? elementId = null)
        {
            // Utwórz instanscję
            var message = new Register
            {
                Type = deviceType.ToString(),
                SolvableProblems = solvableProblems,
                ParallelThreads = parallelThreads,
                Deregister = deregister
            };

            // Opcjonalne
            if (elementId != null)
                message.Id = (ulong) elementId;

            // Zwróć
            return message;
        }
    }
}