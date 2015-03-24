using Common.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer.Control
{
    public class ServerUserInterface
    {
        private static char[] WHITESPACES = new char[] { ' ', '\t', '\n' };
        private const string TIME_PARAMETER = "-t";
        private const string BACKUP_PARAMETER = "-backup";
        private const string PORT_PARAMETER = "-port";
        static void Main(string[] args)
        {
            CommunicationServer communicationServer = new CommunicationServer();
            Console.WriteLine("Communication Server started successfully");
            String newLine;
           
            while (communicationServer.IsWorking)
            {
                newLine = Console.ReadLine();
                communicationServer.CommunicationServerInfo = ServerUserInterface.ReadParameters(newLine);
            }
        }
        static public CommunicationInfo ReadParameters(string s)
        {

            if (s == null) return null;
            string[] parameters = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            CommunicationInfo csP = new CommunicationInfo();
            for (int i = 0; i < parameters.Length; i++ )
            {
                if (parameters[i] == TIME_PARAMETER && i < parameters.Length-1)
                {
                    ulong time;
                    UInt64.TryParse(parameters[i + 1], out time);
                    csP.Time = time;
                }
                else if(parameters[i] == BACKUP_PARAMETER)
                {
                    csP.IsBackup = true;
                }
                else if(parameters[i] == PORT_PARAMETER && i < parameters.Length-1)
                {
                    uint port;
                    UInt32.TryParse(parameters[i + 1], out port);
                    csP.CommunicationServerPort = (ushort)port;
                }
            }
            return csP;
        }
    }
}
