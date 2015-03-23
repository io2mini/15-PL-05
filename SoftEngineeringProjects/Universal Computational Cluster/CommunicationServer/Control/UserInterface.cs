using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemComponent.Control
{
    public class UserInterface
    {
        private static char[] WHITESPACES = new char[] { ' ', '\t', '\n' };
        private static string TIME_PARAMETER = "-t";
        private static string BACKUP_PARAMETER = "-backup";
        private static string PORT_PARAMETER = "-port";

        public string[] ReadParameters(string s)
        {
            if (s == null) return null;
            string[] parameters = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            CommunicationServerParameters csP = new CommunicationServerParameters();
            for (int i = 0; i < parameters.Length; i++ )
            {
                if (parameters[i] == TIME_PARAMETER)
                {

                }
            }
            return parameters;
        }
    }
}
