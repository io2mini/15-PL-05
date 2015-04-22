using System;
using DVRP;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DVRPTest
{
    [TestClass]
    public class DVRPTests
    {
        [TestMethod]
        public void PermutationTest()
        {
            uint n = 4;
            uint[][] testPermutations = Permuter.GeneratePermutations(n);
            Assert.IsTrue(MathExtensions.Factorial(n) == testPermutations.Length);
        }

        [TestMethod]
        public void RecursivePermutationTest()
        {
            uint n = 4;
            uint[][] testPermutations = Permuter.GeneratePermutationsRecursively(n);
            Assert.IsTrue(MathExtensions.Factorial(n) == testPermutations.Length);
        }
    }
}
