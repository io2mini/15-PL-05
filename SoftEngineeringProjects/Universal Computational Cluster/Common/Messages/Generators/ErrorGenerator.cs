using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messages.Generators
{
    public class ErrorGenerator
    {
        public static Error Generate(String errorMessage, ErrorErrorType errorType)
        {
            // Utwórz instanscję
            Error message = new Error();

            // Uzupełnij pola
            message.ErrorMessage = errorMessage;
            message.ErrorType = errorType;

            // Zwróć
            return message;
        }
    }
}
