using System;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;

namespace Common.UserInterface
{
    public class NodeUserInterface
    {
        private static void Main(string[] args)
        {
            var computationalNode = new ComputationalNode();
            Console.WriteLine("Computational Node started successfully");
            string newLine;
            var hasData = false;
            while (computationalNode.IsWorking && !hasData)
            {
                newLine = Console.ReadLine();
                try
                {
                    computationalNode.CommunicationInfo = ParametersParser.ReadParameters(newLine,
                        SystemComponentType.ComputationalNode);
                    hasData = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
                }
            }
            computationalNode.Start();
            while (computationalNode.IsWorking)
            {
            }
            Console.WriteLine("Computational Node ended successfully");
        }
    }
}