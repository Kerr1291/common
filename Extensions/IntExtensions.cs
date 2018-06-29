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
    }
}