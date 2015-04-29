using System;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;

namespace Common.UserInterface
{
    public class ServerUserInterface
    {
        private static void Main(string[] args)
        {
            var communicationServer = new CommunicationServer();
            Console.WriteLine("Communication Server started successfully");
            string newLine;
            var hasBeenRead = false;

            while (communicationServer.IsWorking && !hasBeenRead)
            {
                newLine = Console.ReadLine();
                if (newLine.Length == 0)
                {
                    if (!hasBeenRead)
                    {
                        communicationServer.LoadConfig(SystemComponent.Path);
                        communicationServer.InitializeIPList();
                        hasBeenRead = true;
                    }
                    break;
                }
                try
                {
                    communicationServer.CommunicationInfo = ParametersParser.ReadParameters(newLine,
                        SystemComponentType.CommunicationServer);
                    communicationServer.InitializeIPList();
                    hasBeenRead = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine("Wrong Arguments");
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