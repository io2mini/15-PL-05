using System;
using System.Collections.Generic;

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
            return GeneratePermutationsRecursively(n);
            //_permutations = new List<uint[]>();

            //_permutation = new uint[n];
            //for (uint i = 1; i <= n; i++)
            //{
            //    _permutation[i - 1] = i;
            //}

            ////http://www.drzewo-wiedzy.pl/?page=artykul&id=48
            ////http://www.cut-the-knot.org/do_you_know/AllPerm.shtml
            ////http://sirjoker.w.interia.pl/mat/dyskr/Algorytmy.pdf
            ////http://main.edu.pl/pl/user.phtml?op=lesson&n=34&page=algorytmika
            ////http://www.softwareandfinance.com/CSharp/Permutation_Algorithm.html

            //_permutations.Add(new uint[n]);
            //Array.Copy(_permutation, _permutations[_permutations.Count - 1], n);

            //return _permutations.ToArray();
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

        public static uint[][] GenerateCombinations(uint n, uint k)
        {
            var combinations = new List<uint[]>();
            GenerateCombinationsRecursively(n, k, 0, new uint[n], ref combinations);
            return combinations.ToArray();
        }

        private static void GenerateCombinationsRecursively(uint n, uint k, uint index, uint[] tab,
            ref List<uint[]> tabs)
        {
            if (index == n)
            {
                var tab_prim = new uint[tab.Length];
                for (var i = 0; i < tab_prim.Length; i++)
                    tab_prim[i] = tab[i];
                tabs.Add(tab_prim);
                return;
            }
            for (var i = 0; i < k; i++)
            {
                tab[index] = (uint) i;
                GenerateCombinationsRecursively(n, k, index + 1, tab, ref tabs);
            }
        }

        private static void GenerateNextPermutation(uint k)
        {
            if (_n == k)
            {
                _permutations.Add(new uint[_n]);
                Array.Copy(_permutation, _permutations[_permutations.Count - 1], _n);
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

        public static bool[][] GenerateBooleanCombination(uint length, uint howManyDivides = 0)
        {
            var l = new List<bool[]>();
            var tab = new bool[length];
            GenerateBooleanCombinationRecursively(1, tab, howManyDivides, 1, ref l);
            return l.ToArray();
        }

        private static void GenerateBooleanCombinationRecursively(int actualIndex, bool[] tab, uint howManyDivides, uint actualdivides, ref List<bool[]> resultList)
        {
            if (actualIndex == tab.Length || actualdivides==howManyDivides)
            {
                var t = new bool[tab.Length];
                for (int i = 0; i < tab.Length; i++)
                {
                    t[i] = tab[i];
                }
                resultList.Add(t);
                return;
            }
            tab[actualIndex] = true;
            GenerateBooleanCombinationRecursively(actualIndex + 1, tab, howManyDivides, actualdivides + 1,
                ref resultList);
            tab[actualIndex] = false;
            GenerateBooleanCombinationRecursively(actualIndex+1, tab, howManyDivides, actualdivides, ref resultList);
        }
    }
}