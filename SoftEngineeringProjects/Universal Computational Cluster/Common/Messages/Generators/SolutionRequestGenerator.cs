using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages.Generators
{
    public static class SolutionRequestGenerator
    {
        public static SolutionRequest Generate(ulong Id)
        {
            // Utwórz instancję.
            SolutionRequest msg = new SolutionRequest();
            // Uzupełnij pola.
            msg.Id = Id;
            //Zwróć instancję.
            return msg;
        }
    }
}
