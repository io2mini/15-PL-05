using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemComponent.Control
{
    public class UserInterface
    {
        private static int MAX_NUMBER_PARAMETERS = 3;
        public string[] ReadParameters(string s)
        {
            if (s == null) return null;
            string[] parameters = s.Split(new String[] {"//s"}, MAX_NUMBER_PARAMETERS + 2, StringSplitOptions.RemoveEmptyEntries);
            return parameters;
        }
    }
}
