using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Pow2(this double val)
        {
            return val * val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Pow(this double val, double pow)
        {
            return Math.Pow(val, pow);
        }

        public static double Sqrt(this double val)
        {
            return Math.Sqrt(val);
        }
    }
}
