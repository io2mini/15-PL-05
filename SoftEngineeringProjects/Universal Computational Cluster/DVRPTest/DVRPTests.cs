using System;
using System.Collections.Generic;
using System.IO;
using DVRP;
using DVRP.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

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
            var problemFileUri = new Uri(Directory.GetCurrentDirectory()+"/../../../../../Documentation/io2_8_plain_a_D.vrp");
            // Utwórz nowy problem
            DVRP.Problem p = DVRP.Problem.CreateProblemInstanceFromFile(problemFileUri);
            DVRP.TaskSolver ts = new DVRP.TaskSolver(p.Serialize());
            var ttb = ts.DivideProblem(8);
            byte[][] solutions = new byte[ttb.GetLength(0) - 1][];
            List<Thread> threads = new List<Thread>();
            Mutex WaitForThread = new Mutex();
            for (int i = 1; i < ttb.GetLength(0); i++)
            {
                threads.Add(new Thread((object o) =>
                {
                    
                    DVRP.TaskSolver tsn = new TaskSolver(ttb[0]);
                    WaitForThread.WaitOne();
                    solutions[((int)o)-1] = (tsn.Solve(ttb[(int)o], new TimeSpan()));
                    WaitForThread.ReleaseMutex();
                }));
            }
            for(int i=1;i<threads.Count+1;i++)
            {
                threads[i-1].Start(i);
            }
            foreach (var I in threads) I.Join();
            var ms = ts.MergeSolution(solutions);
            var s = Solution.Deserialize(ms);
            var cost = s.Cost;
        }
    }
}