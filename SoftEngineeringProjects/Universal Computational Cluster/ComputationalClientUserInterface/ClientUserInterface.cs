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
            ComputationalClient computationalNode = new ComputationalClient();
            Console.WriteLine("Computational Client started successfully");
            String newLine;
            while (computationalNode.IsWorking)
            {
                newLine = Console.ReadLine();
                computationalNode.CommunicationInfo = ParametersParser.ReadParameters(newLine);
            }
        }
    }
}
