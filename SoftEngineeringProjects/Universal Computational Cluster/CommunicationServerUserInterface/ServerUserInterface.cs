using Common.Communication;
using Common.Components;
using Common.Exceptions;
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
                        hasBeenRead = true;
                    }
                    break;
                }
                try
                {
                    communicationServer.CommunicationInfo = ParametersParser.ReadParameters(newLine, SystemComponentType.CommunicationServer);
                    communicationServer.CommunicationInfo.CommunicationServerAddress = new Uri("http://25.150.46.216/");
                    hasBeenRead = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
                    continue;
                }
                
            }
            communicationServer.Start();

            // Osbluga komendy zakonczenia programu
            while (communicationServer.IsWorking)
            {

            }
        }
    }
}
