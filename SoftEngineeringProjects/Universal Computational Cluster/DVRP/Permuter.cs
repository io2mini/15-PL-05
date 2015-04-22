using System;
using System.Collections.Generic;
using System.Linq;

namespace DVRP
{
    public static class Permuter
    {
        public static int[][] GeneratePermutations(uint n)
        {
            List<int[]> permutations = new List<int[]>();

            int[] a = new int[n];
            for (int i = 1; i <= n; i++)
            {
                a[i-1] = i;
            }

            // Indesator kolejnych pozycji
            int index = 0;

            //http://www.drzewo-wiedzy.pl/?page=artykul&id=48
            //http://www.cut-the-knot.org/do_you_know/AllPerm.shtml
            //http://sirjoker.w.interia.pl/mat/dyskr/Algorytmy.pdf
            //http://main.edu.pl/pl/user.phtml?op=lesson&n=34&page=algorytmika
            //http://www.softwareandfinance.com/CSharp/Permutation_Algorithm.html

            permutations.Add(new int[n]);
            Array.Copy(a, permutations[index++], n);
            return permutations.ToArray();
        }
    }
}