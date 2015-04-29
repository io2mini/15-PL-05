namespace Common.Messages.Generators
{
    public class ErrorGenerator
    {
        public static Error Generate(string errorMessage, ErrorErrorType errorType)
        {
            // Utwórz instanscję
            var message = new Error
            {
                ErrorMessage = errorMessage,
                ErrorType = errorType
            };

            // Zwróć
            return message;
        }
    }
}