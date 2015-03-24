using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
    public class ParametersParser
    {
        private static char[] WHITESPACES = new char[] { ' ', '\t', '\n' };
        private const string TIME_PARAMETER = "-t";
        private const string BACKUP_PARAMETER = "-backup";
        private const string PORT_PARAMETER = "-port";
        private const string ADDRESS_PARAMTER = "-address";

        static public CommunicationInfo ReadParameters(string s)
        {
            if (s == null) return null;
            string[] parameters = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            CommunicationInfo cInfo = new CommunicationInfo();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == TIME_PARAMETER && i < parameters.Length - 1)
                {
                    ulong time;
                    UInt64.TryParse(parameters[i + 1], out time);
                    cInfo.Time = time;
                }
                else if (parameters[i] == BACKUP_PARAMETER)
                {
                    cInfo.IsBackup = true;
                }
                else if (parameters[i] == ADDRESS_PARAMTER)
                {
                    cInfo.CommunicationServerAddress = new Uri(parameters[i]);
                }
                else if (parameters[i] == PORT_PARAMETER && i < parameters.Length - 1)
                {
                    uint port;
                    UInt32.TryParse(parameters[i + 1], out port);
                    cInfo.CommunicationServerPort = (ushort)port;
                }
            }
            return cInfo;
        }
    }
}
