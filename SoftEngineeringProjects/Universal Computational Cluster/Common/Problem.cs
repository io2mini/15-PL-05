using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Problem
    {
        private String problemType;
        private ulong solvingTimeOut;
        private byte[] serializedProblem;
        private ulong problemInstanceId;

        public Problem()
        {

        }

        public byte[] SerializedProblem
        {
            get { return serializedProblem; }
            set { serializedProblem = value; }
        }

        public ulong SolvingTimeOut
        {
            get { return solvingTimeOut; }
            set { solvingTimeOut = value; }
        }

        public ulong ProblemInstanceId
        {
            get { return problemInstanceId; }
            set { problemInstanceId = value; }
        }

        public string ProblemType
        {
            get { return problemType; }
            set { problemType = value; }
        }
    }
}
