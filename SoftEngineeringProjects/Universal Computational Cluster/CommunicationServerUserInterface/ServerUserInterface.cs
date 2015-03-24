using Common.Communication;
using Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UserInterface
{
    public class ServerUserInterface
    {
        static void Main(string[] args)
        {
            CommunicationServer communicationServer = new CommunicationServer();
            Console.WriteLine("Communication Server started successfully");
            String newLine;
         
            while (communicationServer.IsWorking)
            {
                newLine = Console.ReadLine();
                communicationServer.CommunicationInfo = ParametersParser.ReadParameters(newLine);
            }
        }
    }
}
