using System.IO;
using Common.Configuration;
using Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserInterface
{
    public class ClientUserInterface
    {
        static void Main(string[] args)
        {
            ComputationalClient computationalClient = new ComputationalClient();
            Console.WriteLine("Computational Client started successfully");
            String newLine;
            bool hasBeenRead = false;
            while (computationalClient.IsWorking && !hasBeenRead)
            {
                newLine = Console.ReadLine();
                computationalClient.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.ComputationalClient);
                hasBeenRead = true;
            }

            // Rozpocznij pobieranie informacje o plikach do wczytania
            bool existingProblem = computationalClient.ProblemExists();
            Problem newProblem = new Problem();
            if (computationalClient.IsWorking &&!existingProblem)
            {
                Console.Error.WriteLine("Not implemented:");

                // Utwórz nowy problem
                

                // Podaj problem type
                Console.WriteLine("Type problem type name:");
                newLine = Console.ReadLine();
                newProblem.ProblemType = newLine.Trim();

                Console.WriteLine("Type path file of problem instance:");
                newLine = Console.ReadLine();
                // Szybki pars
              
                // Utwórz nowe uri
                Uri problemFileUri = new Uri(newLine);
                newProblem.SerializedProblem = File.ReadAllBytes(problemFileUri.AbsolutePath);
                Console.WriteLine("OK. Problem instance is ready.");

                // Pobierz czas oczekiwania na rozwiazanie
                Console.WriteLine("OPTIONAL: Time restriction for solving the problem (in ms) or N:");
                newLine = Console.ReadLine();
                if (newLine.Trim() != null && !(newLine.Trim().Equals("N") || newLine.Trim().Equals("n")))
                {
                    newProblem.SolvingTimeOut = ulong.Parse(newLine.Trim());
                }

                
            }
            computationalClient.Start(newProblem, existingProblem);
            Console.WriteLine("Computational Client ended successfully");
        }
    }
}
