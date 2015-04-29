using System;
using DVRP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DVRP.Objects;
using System.Drawing;
using System.Collections.Generic;
namespace DVRPTest
{
    [TestClass]
    public class DVRPTests
    {
        [TestMethod]
        public void PermutationTest()
        {
            //uint n = 4;
            //uint[][] testPermutations = Permuter.GeneratePermutations(n);
            //Assert.IsTrue(MathExtensions.Factorial(n) == testPermutations.Length);
        }

        [TestMethod]
        public void RecursivePermutationTest()
        {
            uint n = 4;
            uint[][] testPermutations = Permuter.GeneratePermutationsRecursively(n);
            Assert.IsTrue(MathExtensions.Factorial(n) == testPermutations.Length);
        }

        [TestMethod]
        public void ProblemSerializationExpectedNoExceptionsTest()
        {
            Problem problem = new Problem(
                new List<Vehicle>() 
                {
                    new Vehicle(new Location(1,1), 10, 10)
                },
                new List<Client>()
                {
                    new Client(new Location(3,3), new TimeSpan(2,3,4), new TimeSpan(4,4,4), 2, 2)
                },           
                new List<Depot>() 
                { 
                    new Depot(new Location(2,2), new TimeSpan(1,10,1), new TimeSpan(1,11,11))
                },
                new int[2][]);
            String problemString = problem.ToString();
            byte[] serialized = problem.Serialize();
            Assert.AreEqual(Problem.Deserialize(serialized).ToString(), problemString);
        }
    }
}
