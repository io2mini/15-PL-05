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
        }
    }
}
