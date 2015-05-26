using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Common.Exceptions;
using Common.Messages;
using Common.Messages.Generators;
using Common.Properties;
using DVRP;

namespace Common.Components
{
    /// <summary>
    ///     Informacja ogólna: ComputatonalClient nie może być rozpatrywany jako SystemComponent (jako taki)
    ///     Należy rozważyć bardziej szczegółową hierarchię komponentów.
    /// </summary>
    public class ComputationalClient : SystemComponent
    {
        private const string SolveRequestResponse = "SolveRequestResponse";
        private ulong ProblemID;
        private Thread SolutionRequester;
        private bool ExistingProblem { get; set; }
        public bool HasFinalSolution { get; private set; }

        public void SendSolveRequestMessage(Problem problem)
        {
            var msg = SolveRequestGenerator.Generate(problem.ProblemType, problem.SerializedProblem,
                problem.SolvingTimeOut, problem.ProblemInstanceId);
            SendMessage(msg);
        }

        protected override void Initialize()
        {
            base.Initialize();
            //SolveRequestResponse
            SchemaTypes.Add(SolveRequestResponse,
                new Tuple<string, Type>(Resources.SolveRequestResponse, typeof (SolveRequestResponse)));
        }

        public void Start(Problem problem, bool existingProblem)
        {
            InitializeConnection();
            ExistingProblem = existingProblem;
            if (!ExistingProblem) CreateProblem(problem);
            else
            {
                var F = new StreamReader(new FileStream("Problem.dat", FileMode.Open));
                ProblemID = ulong.Parse(F.ReadLine());
                F.Close();
                AskForSolutions();
            }
        }

        private void SendSolutionRequest()
        {
            try
            {
                while (IsWorking && ExistingProblem)
                {
                    SendMessage(SolutionRequestGenerator.Generate(ProblemID));
                    ReceiveResponse();
                }
            }
            catch (MessageNotSentException)
            {
                Console.WriteLine("Message Not send for component type {0} with id {1}", DeviceType, Id);
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
            var t = new Thread(ReceiveResponse);
            t.IsBackground = true;
            t.Start();
        }

        protected override void HandleMessage(Message message, string key, Socket socket)
        {
            switch (key)
            {
                case SolveRequestResponse:
                    MsgHandler_SolveRequestResponse((SolveRequestResponse) message, socket);
                    return;
                default:
                    base.HandleMessage(message, key, socket);
                    return;
            }
        }

        /// <summary>
        /// Metoda odbierająca odpowiedź na solution request - odpowiedzią jest Solution
        /// </summary>
        /// <param name="solutions"></param>
        protected override void MsgHandler_Solution(Solutions solutions)
        {
            foreach (var solutionsSolution in solutions.Solutions1)
            {
                Console.WriteLine(solutionsSolution.Type.ToString());

                if (solutionsSolution.TimeoutOccured)
                {
                    Console.WriteLine("Solution wasn't found because of TimeOut parameter.");
                    HasFinalSolution = true;
                    break;
                }

                if (solutionsSolution.Type != SolutionsSolutionType.Final) 
                    continue;

                Console.WriteLine("Here is your final solution:");
                Solution solution = DVRP.Solution.Deserialize(solutionsSolution.Data);
                Console.WriteLine("Final cost: {0}", solution.Cost);
                Console.WriteLine("Route:");
                for (int i = 0; i < solution.VehicleLocationList.Count; i++)
                {
                    Console.Write("{0}  ", solution.VehicleLocationList[i]);
                }
                Console.WriteLine("\nFinding this solution takes approximately: {0}", solutionsSolution.ComputationsTime);

                HasFinalSolution = true;
                break;
            }
        }

        protected void MsgHandler_SolveRequestResponse(SolveRequestResponse solveRequestResponse, Socket socket)
        {
            ProblemID = solveRequestResponse.Id;
            ExistingProblem = true;
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