using System;
using System.Collections.Generic;
using System.Linq;

namespace DVRP
{
    public static class Permuter
    {
        private static bool[] _used;
        private static uint[] _permutation;
        private static List<uint[]> _permutations;
        private static uint _n;

        public static uint[][] GeneratePermutations(uint n)
        {
            _permutations = new List<uint[]>();

            _permutation = new uint[n];
            for (uint i = 1; i <= n; i++)
            {
                _permutation[i - 1] = i;
            }

            //http://www.drzewo-wiedzy.pl/?page=artykul&id=48
            //http://www.cut-the-knot.org/do_you_know/AllPerm.shtml
            //http://sirjoker.w.interia.pl/mat/dyskr/Algorytmy.pdf
            //http://main.edu.pl/pl/user.phtml?op=lesson&n=34&page=algorytmika
            //http://www.softwareandfinance.com/CSharp/Permutation_Algorithm.html

            _permutations.Add(new uint[n]);
            Array.Copy(_permutation, _permutations[_permutations.Count - 1], n);

            return _permutations.ToArray();
        }

        public static uint[][] GeneratePermutationsRecursively(uint n)
        {
            _used = new bool[n];
            _permutation = new uint[n];
            _permutations = new List<uint[]>();
            _n = n;

            GenerateNextPermutation(0);

            return _permutations.ToArray();
        }

        private static void GenerateNextPermutation(uint k)
        {
            if (_n == k)
            {
                _permutations.Add(new uint[_n]);
                Array.Copy(_permutation, _permutations[_permutations.Count-1], _n);
                return;
            }
            for (uint m = 0; m < _n; m++)
            {
                if (!_used[m])
                {
                    _used[m] = true;
                    _permutation[k] = m;
                    GenerateNextPermutation(k + 1);
                    _used[m] = false;
                }
            }      
        }
    }
}