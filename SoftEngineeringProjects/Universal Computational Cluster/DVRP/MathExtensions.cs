using System;

namespace DVRP
{
    public static class MathExtensions
    {
        public static uint Factorial(uint n)
        {
            uint silnia = 1;
            for (uint i = 1; i <= n; i++)
            {
                silnia *= i;
            }
            return silnia;
        }
    }
}