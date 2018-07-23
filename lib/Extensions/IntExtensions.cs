using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public static class IntExtensions
    {
        public static string ToHexString(this int val)
        {
            return val.ToString("X");
        }

        public static int GetFirstFlippedBitIndex(this int b)
        {
            for(int i = 0; i < 32; ++i)
            {
                bool r = ((1 << i) & b) == 0;
                if(!r)
                    return i;
            }
            return -1;
        }
    }
}