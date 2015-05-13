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

        public static int[][] GenerateLengthBrackets(int divideCount, int desiredSum)
        {
            var l = new List<List<int>> { new List<int>{1, int.MaxValue} };
            for (int i = 2; i <= desiredSum; i++)
            {
                var ll = new List<List<int>>();
                var actList = new List<int> { 1 };
                if (l[0].Count <= divideCount)
                {
                    actList.AddRange(l[0]);
                    ll.Add(actList);
                }
                for (int j = 0; j < l.Count; j++)
                {
                    for (int k = l[j].Count - 1; k >= 0 && l[j][k + 1] > l[j][k]; k--)
                    {
                        actList.Clear();
                        actList.AddRange(l[j]);
                        actList[k]++;
                        ll.Add(actList);
                    }
                }
                l = ll;
            }
            var Result = new List<int[]>();
            foreach (var intlist in l)
            {
                var s = new int[divideCount + 1];
                for (int i = 0; i < intlist.Count - 1; i++)
                {
                    s[intlist[i]]++;
                }
                Result.Add(s);
            }
            return Result.ToArray();
        }
        public static void FillBracketsSingle(int i, List<List<uint>> Brackets, int[] bracketCapacities,ref List<uint[][]> result, int idCount)
        {
            if (i >= idCount)
            {
                List<uint[]> ListofArrays = new List<uint[]>();
                foreach (var L in Brackets) ListofArrays.Add(L.ToArray());
                result.Add(ListofArrays.ToArray());
                return;
            }
            for (int j = 0; j < Brackets.Count; j++)
            {
                if (Brackets[j].Count < bracketCapacities[j]*j)
                {
                    Brackets[j].Add((uint)i);
                    FillBracketsSingle(i + 1, Brackets, bracketCapacities, ref result, idCount);
                    Brackets[j].RemoveAt(Brackets[j].Count - 1);
                }
            }
        }
        public static List<uint[][][]> GenerateAndFillBrackets(int divideCount, int desiredSum)
        {
            var bracketCapacities = GenerateLengthBrackets(divideCount, desiredSum);
            var bracketAssignments = new List<uint[][][]>();
            foreach (var arr in bracketCapacities)
            {
                var bracketAssignment = new List<uint[][]>();
                var brackets = new List<List<uint>>();
                for(int i=0; i<arr.Length; i++) brackets.Add(new List<uint>());
                FillBracketsSingle(0, brackets ,arr, ref bracketAssignment, desiredSum);
                bracketAssignments.Add(bracketAssignment.ToArray());
            }
            //TODO: use PermuteAll
            return bracketAssignments;
        }
        public static void FillBracketLevelRecursively(ref List<uint[][]> Result,List<List<uint>> Brackets, int i, int bracketsize, uint[] array)
        {
            if(i==array.Length)
            {
                List<uint[]> ListofArrays = new List<uint[]>();
                foreach (var L in Brackets) ListofArrays.Add(L.ToArray());
                Result.Add(ListofArrays.ToArray());
                return;
            }
            for (int j = 0; j < Brackets.Count; j++)
            {
                if (Brackets[j].Count < bracketsize) continue;
               
                for (int k = 0; k < Brackets[j].Count; k++)
                {
                    if (Brackets[j][k] < int.MaxValue) continue;
                    Brackets[j][k] = array[i];
                    FillBracketLevelRecursively(ref Result, Brackets, i + 1, bracketsize, array);
                    Brackets[j][k] = int.MaxValue;
                }
                Brackets[j].RemoveAt(Brackets[j].Count - 1);            
            }
        }
        public static uint[][][] PermuteAll(uint[] array,int bracketsize,int bracketcount)
        {
           //TODO: fill in function
            List<uint[][]> Result = new List<uint[][]>();
            List<List<uint>> Brackets = new List<List<uint>>();
            for (int i = 0; i < bracketcount; i++)
            {
                Brackets.Add(new List<uint>());
                for (int j = 0; j < bracketsize; j++) Brackets[i].Add(int.MaxValue);
            }
            
            FillBracketLevelRecursively(ref Result, Brackets, 0, bracketsize, array);
            return Result.ToArray();
        }
        public static uint[][] Permute(uint[] array)
        {
            var indexPermutations = GeneratePermutationsRecursively((uint)array.Length);
            for (int i = 0; i < indexPermutations.GetLength(0); i++)
            {
                for (int j = 0; j < indexPermutations[i].Length; j++)
                    indexPermutations[i][j] = array[indexPermutations[i][j]];
            }
            return indexPermutations;
        }
    }
}