using System;
using System.Collections.Generic;
using System.Linq;

namespace DVRP
{
    public static class Permuter
    {
        public static int[][] GeneratePermutations(int n, int k)
        {
            List<int[]> permutations = new List<int[]>();

            int[] a = new int[n];
            for (int i = 1; i <= n; i++)
            {
                a[i-1] = i;
            }

            int index = 0;
            int p = k;
            while (p  >= 1) {
                permutations.Add(new int[n]);
                Array.Copy(a, permutations[index++], n);

                if (a[k-1] == n) p--;
                else p = n;
                if (p >= 1) 
                {
                    for (int i = k; i >= p; i--)
                    {
                        a[i-1] = a[p-1] + i - p  - 1;
                    }
                }
            }
            return permutations.ToArray();
        }
    }
}