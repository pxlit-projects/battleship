using System;

namespace Battleship.TestTools
{
    public static class RandomExtensions
    {
        public static bool NextBool(this Random random)
        {
            return random.Next(0, 2) == 1;
        }
    }
}
