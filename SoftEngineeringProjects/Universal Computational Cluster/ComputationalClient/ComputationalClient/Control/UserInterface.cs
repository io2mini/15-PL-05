using System;
using SystemComponent.Control.Helpers;

namespace SystemComponent.Control
{
    public class UserInterface
    {
        private static char[] WHITESPACES = new char[] { ' ', '\t', '\n' };

        /// <summary>
        /// Metoda pobierająca ciąg wejściowy w jednej linijce i zamieniająca go na tabelę parametrów wejściowych.
        /// </summary>
        /// <param name="s">String zawierający jedną linię do przetwożenia.</param>
        /// <returns>Zwraca obiekt parametrów wejściowych gotowych do dalszego procesingu.</returns>
        public CommandLineParameters ReadParameters(string s)
        {
            if (s == null) return null;

            // Podziel string 
            string[] tempStrings = s.Split(WHITESPACES, StringSplitOptions.RemoveEmptyEntries);
            if (tempStrings.Length < 4) return null;

            // Wykonaj walidację wartości
            CommandLineParameters clp = null;
            if (String.Equals("-address", tempStrings[0]) && String.Equals("-port", tempStrings[2]))
            {
                Uri tempUri = new Uri(tempStrings[1]);
                ushort tempUshort = (ushort)Convert.ToUInt32(tempStrings[3]);
                clp = new CommandLineParameters(tempUri, tempUshort);
            }

            // Zwróć obiekt
            return clp;
        }
    }
}