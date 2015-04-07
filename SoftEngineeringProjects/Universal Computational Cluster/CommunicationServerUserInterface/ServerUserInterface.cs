using Common.Communication;
using Common.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            string newLine;
            bool hasBeenRead = false;
            while (communicationServer.IsWorking && !hasBeenRead)
            {
                newLine = Console.ReadLine();
                if (newLine.Length == 0)
                {
                    if (!hasBeenRead)
                    {
                        communicationServer.LoadConfig(SystemComponent.Path);
                    }
                    break;
                }
                communicationServer.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.CommunicationServer);
                communicationServer.CommunicationInfo.CommunicationServerAddress = new Uri("http://127.0.0.1/");
                hasBeenRead = true;
            }
            communicationServer.Start();

            // Osbluga komendy zakonczenia programu
            while (communicationServer.IsWorking)
            {
                int m = 0;
            }
        }
    }
}
