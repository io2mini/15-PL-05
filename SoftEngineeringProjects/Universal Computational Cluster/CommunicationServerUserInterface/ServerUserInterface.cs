using System;
using Common.Components;
using Common.Configuration;
using Common.Exceptions;
using Common.Properties;
using System.Collections.Generic;

namespace Common.UserInterface
{
    public class ServerUserInterface
    {
        private static void Main(string[] args)
        {
            var communicationServer = new CommunicationServer();
            Console.WriteLine(Resources.ServerUserInterface_Main_Communication_Server_started_successfully);
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
                        communicationServer.InitializeIpList();
                        hasBeenRead = true;
                    }
                    break;
                }
                try
                {
                    List<CommunicationInfo> communicationInfos = ParametersParser.ReadParameters(newLine,
                        SystemComponentType.CommunicationServer);
                    if (communicationInfos.Count > 1)
                    {
                        communicationServer.CommunicationServerInfo = communicationInfos[1];
                        communicationServer.MyCommunicationInfo = communicationInfos[0];
                        communicationServer.IsPrimary = false;
                    }
                    else
                    {
                        communicationServer.CommunicationServerInfo = communicationInfos[0];
                        communicationServer.MyCommunicationInfo = communicationInfos[0];
                        communicationServer.IsPrimary = true;
                    }
                    communicationServer.InitializeIpList();
                    hasBeenRead = true;
                }
                catch (ParsingArgumentException)
                {
                    Console.WriteLine(Resources.ServerUserInterface_Main_Wrong_Arguments);
                }
            }
            communicationServer.Start();

            // Osbluga komendy zakonczenia programu

        }
    }
}