namespace Common
{
    public class Problem
    {
        public byte[] SerializedProblem { get; set; }
        public ulong SolvingTimeOut { get; set; }
        public ulong ProblemInstanceId { get; set; }
        public string ProblemType { get; set; }
    }
}