using Common.Communication;
using Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserInterface
{
    public class NodeUserInterface
    {
        static void Main(string[] args)
        {
            ComputationalNode computationalNode = new ComputationalNode();
            Console.WriteLine("Computational Node started successfully");
            String newLine;
            while (computationalNode.IsWorking)
            {
                newLine = Console.ReadLine();
                computationalNode.CommunicationInfo = ParametersParser.ReadParameters(newLine);
            }
        }
    }
}
