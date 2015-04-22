using Common.Configuration;
using Common.Components;
using Common.Exceptions;
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
            bool hasData = false;
            while (computationalNode.IsWorking && !hasData)
            {
                newLine = Console.ReadLine();
                try
                {
                    computationalNode.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.ComputationalNode);
                    hasData = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
                    continue;
                }
               
            }
            computationalNode.Start();
            while (computationalNode.IsWorking) { }
            Console.WriteLine("Computational Node ended successfully");
        }
    }
}
