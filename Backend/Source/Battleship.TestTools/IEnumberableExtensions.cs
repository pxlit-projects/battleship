using System;
using System.Collections.Generic;
using System.Linq;

namespace Battleship.TestTools
{
    public static class IEnumberableExtensions
    {
        private static readonly Random RandomGenerator = new Random();

        public static T NextRandomElement<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            int index = RandomGenerator.Next(list.Count);
            return list.ElementAt(index);
        }

        public static T NextRandomElement<T>(this IEnumerable<T> enumerable, out int index)
        {
            var list = enumerable.ToList();
            index = RandomGenerator.Next(list.Count);
            return list.ElementAt(index);
        }
    }
}