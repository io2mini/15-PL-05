using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemComponent.Control;
using SystemComponent.Control.Helpers;

namespace SystemComponent
{
    class ComputationalClient
    {
        private static bool isWorking;

        public ComputationalClient()
        {
            isWorking = true;
        }

        static void Main(string[] args)
        {
            // Inicjalizacja klienta
            Console.WriteLine("Ok, Computational Client is ready.");

            // Odczytuj polecenia z konsoli i przetwazaj je
            UserInterface uI = new UserInterface();
            CommandLineParameters clp;
            while (isWorking)
            {
                string tempLine = Console.ReadLine();
                clp = uI.ReadParameters(tempLine);
            }
        }
    }
}
