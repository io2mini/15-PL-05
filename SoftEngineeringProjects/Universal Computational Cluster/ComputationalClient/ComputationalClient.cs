using System;
using Common.Messages;
using Common.Messages.Generators;
using Common.Properties;
using System.Net.Sockets;

namespace Common.Components
{
    /// <summary>
    /// Informacja ogólna: ComputatonalClient nie może być rozpatrywany jako SystemComponent (jako taki)
    /// Należy rozważyć bardziej szczegółową hierarchię komponentów.
    /// </summary>
    public class ComputationalClient : SystemComponent
    {
        const string SolveRequestResponse = "SolveRequestResponse";
        public ComputationalClient() : base() { }

        public void SendSolveRequestMessage(Problem problem)
        {
            SolveRequest msg = SolveRequestGenerator.Generate(problem.ProblemType, problem.SerializedProblem, 
                problem.SolvingTimeOut, problem.ProblemInstanceId);
            SendMessage(msg);
        }

        protected override void Initialize()
        {
            base.Initialize();
            //SolveRequestResponse
            SchemaTypes.Add(SolveRequestResponse, new Tuple<string, Type>(Resources.SolveRequestResponse, typeof(SolveRequestResponse)));
        }

        protected override void HandleMessage(Messages.Message message, string key, Socket socket)
        {
            switch (key)
            {
                case SolveRequestResponse:
                    MsgHandler_SolveRequestResponse((SolveRequestResponse)message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }

        }

        private void MsgHandler_SolveRequestResponse(Messages.SolveRequestResponse solveRequestResponse, Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}
