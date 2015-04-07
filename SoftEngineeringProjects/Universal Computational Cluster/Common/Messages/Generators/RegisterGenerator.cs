namespace Common.Messages.Generators
{
    public static class RegisterGenerator
    {
        public static Register Generate(SystemComponentType deviceType, string[] solvableProblems, byte parallelThreads, bool deregister = false, ulong ?elementId = null)
        {
            // Utwórz instanscję
            Register message = new Register();

            // Uzupełnij pola
            message.Type = deviceType.ToString();
            message.SolvableProblems = solvableProblems;
            message.ParallelThreads = parallelThreads;

            // Opcjonalne
            message.Deregister = deregister;
            if (elementId != null)
                message.Id = (ulong)elementId;

            // Zwróć
            return message;
        }
    }
}