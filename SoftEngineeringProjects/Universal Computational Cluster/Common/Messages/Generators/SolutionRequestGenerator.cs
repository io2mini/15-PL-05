namespace Common.Messages.Generators
{
    public static class SolutionRequestGenerator
    {
        public static SolutionRequest Generate(ulong Id)
        {
            // Utwórz instancję.
            var msg = new SolutionRequest {Id = Id};
            //Zwróć instancję.
            return msg;
        }
    }
}