using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class RNG
    {
        private static Random generator = new Random();

        public static int NumberBetween(int minimumValue, int maximumValue)
        {
            return generator.Next(minimumValue, maximumValue +1);
        }
    }
}
