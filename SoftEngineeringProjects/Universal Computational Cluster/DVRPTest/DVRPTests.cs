using System;
using System.Collections.Generic;
using DVRP;
using DVRP.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DVRPTest
{
    [TestClass]
    public class DvrpTests
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
            var testPermutations = Permuter.GeneratePermutationsRecursively(n);
            Assert.IsTrue(MathExtensions.Factorial(n) == testPermutations.Length);
        }

        [TestMethod]
        public void ProblemSerializationExpectedNoExceptionsTest()
        {
            var problem = new Problem(
                new List<Vehicle>
                {
                    new Vehicle(new Location(1, 1), 10, 10)
                },
                new List<Client>
                {
                    new Client(new Location(3, 3), new TimeSpan(2, 3, 4), new TimeSpan(4, 4, 4), 2, 2, 1)
                },
                new List<Depot>
                {
                    new Depot(new Location(2, 2), new TimeSpan(1, 10, 1), new TimeSpan(1, 11, 11), 2)
                },
                new int[2][]);
            var problemString = problem.ToString();
            var serialized = problem.Serialize();
            Assert.AreEqual(Problem.Deserialize(serialized).ToString(), problemString);
        }

        [TestMethod]
        public void DVRPComputationTest()
        {
            var problemFileUri = new Uri(@"C:\Users\nowaks\Downloads\io2_8b_.vrp");
            // Utwórz nowy problem
            DVRP.Problem p = DVRP.Problem.CreateProblemInstanceFromFile(problemFileUri);
            DVRP.TaskSolver ts = new DVRP.TaskSolver(p.Serialize());
            var ttb = ts.DivideProblem(8);
            List<byte[]> solutions = new List<byte[]>();
            for (int i = 1; i < ttb.GetLength(0); i++)
            {
                DVRP.TaskSolver tsn = new TaskSolver(ttb[0]);
                solutions.Add(tsn.Solve(ttb[i], new TimeSpan()));
            }
            var ms = ts.MergeSolution(solutions.ToArray());
            var s = Solution.Deserialize(ms);
            var cost = s.Cost;
        }
    }
}