using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemComponent.Control;

namespace SystemComponent
{
    public class CommunicationServer
    {
        private static bool isWorking = true;

        public bool IsWorking { get; set; }

        public CommunicationServer()
        {
            isWorking = true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Communication Server started successfully");
            String newLine;
            UserInterface uI = new UserInterface();
            while (isWorking)
            {
                newLine = Console.ReadLine();
                string[] parameters = uI.ReadParameters(newLine);
            }
        }
    }
}
