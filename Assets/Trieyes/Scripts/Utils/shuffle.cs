using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class ListExtensions
    {
        private static System.Random rng = new System.Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1); // 0 ~ n 사이
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}