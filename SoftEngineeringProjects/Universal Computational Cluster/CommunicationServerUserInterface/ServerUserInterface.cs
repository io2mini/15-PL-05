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
        const string PATH = "";
        static void Main(string[] args)
        {
            CommunicationServer communicationServer = new CommunicationServer();
            Console.WriteLine("Communication Server started successfully");
            String newLine;
            bool hasBeenRead = false;
            while (communicationServer.IsWorking)
            {
                newLine = Console.ReadLine();
                if (newLine.Length == 0)
                {
                    if (!hasBeenRead)
                    {
                        communicationServer.LoadConfig(PATH);
                    }
                    break;
                }
                communicationServer.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.CommunicationServer);
                hasBeenRead = true;
            }
            communicationServer.Start();
        }
    }
}
