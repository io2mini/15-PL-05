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
            Permuter.GeneratePermutations(4);
        }
    }
}
