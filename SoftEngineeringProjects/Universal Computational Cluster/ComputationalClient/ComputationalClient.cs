using System;
using Common.Messages;
using Common.Messages.Generators;
using Common.Properties;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Common.Exceptions;

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
        private ulong ProblemID;
        Thread SolutionRequester;
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
        bool ExistingProblem { get; set; }
        public void Start(Problem problem,bool existingProblem)
        {

            InitializeConnection();
            ExistingProblem = existingProblem;
            if (!ExistingProblem) CreateProblem(problem);
            else
            {
                StreamReader F = new StreamReader(new FileStream("Problem.dat", FileMode.Open));
                ProblemID = ulong.Parse(F.ReadLine());
                F.Close();
                AskForSolutions();
            }
        }
        private void SendSolutionRequest()
        {
            try
            {
                while (this.IsWorking && this.ExistingProblem)
                {
                    SendMessage(SolutionRequestGenerator.Generate(ProblemID));
                    ReceiveResponse();
                }
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine("Message Not send for component type {0} with id {1}", deviceType.ToString(), this.Id);
            }
            catch (Exception)
            {
                //TODO: Obsługa wyjątków socketa
            }
        }
        private void AskForSolutions()
        {
            SolutionRequester = new Thread(SendSolutionRequest);
            SolutionRequester.IsBackground = true;
            SolutionRequester.Start();

        }
        public bool ProblemExists()
        {
            return File.Exists("Problem.dat");
        }
        private void CreateProblem(Problem path)
        {
            

            SendSolveRequestMessage(path);
            Thread t = new Thread(ReceiveResponse);
            t.IsBackground = true;
            t.Start();
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

        protected void MsgHandler_SolveRequestResponse(Messages.SolveRequestResponse solveRequestResponse, Socket socket)
        {
            
            ProblemID = solveRequestResponse.Id;
            AskForSolutions();
        }

        protected override void MsgHandler_Error(Error message)
        {
            if (message.ErrorType != ErrorErrorType.ExceptionOccured)
            {
                base.MsgHandler_Error(message);
                return;
            }
            //TODO: handle received exception
            throw new NotImplementedException();
        }
    }
}
