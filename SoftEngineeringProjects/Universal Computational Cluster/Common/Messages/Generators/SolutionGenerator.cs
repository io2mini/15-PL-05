namespace Common.Messages.Generators
{
    public static class SolutionGenerator
    {
        public static Solutions Generate(byte[] commonData, ulong id, string problemType, SolutionsSolution[] partialSolutions)
        {
            Solutions s = new Solutions()
            {
                CommonData = commonData,
                Id = id,
                ProblemType = problemType,
                Solutions1 = partialSolutions
            };

            return s;
        }
    }
}