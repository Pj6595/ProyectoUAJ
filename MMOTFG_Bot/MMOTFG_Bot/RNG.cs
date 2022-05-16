using System;
using System.Collections.Generic;
using System.Text;

namespace MMOTFG_Bot
{
    static class RNG
    {
        static private Random rng;
        
        static public void Init(int seed = 0)
        {
            if(seed == 0) rng = new Random();
            else rng = new Random(seed);
        }

        static public int Next(int minInclusive, int maxExclusive)
        {
            return rng.Next(minInclusive, maxExclusive);
        }
    }
}
