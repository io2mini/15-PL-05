using Common.Communication;
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
            while (computationalClient.IsWorking)
            {
                newLine = Console.ReadLine();
                computationalClient.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.ComputationalClient);
            }

            // Rozpocznij pobieranie informacje o plikach do wczytania
            while (computationalClient.IsWorking)
            {
                Console.WriteLine("Insert path file of problem instance.");
                newLine = Console.ReadLine();

                
                // TODO: W kolejnych wersjach programu otwórz plik, przetwóż go i rozpocznij wysyłanie,
                // TODO: odbieranie problemu
            }
        }
    }
}
