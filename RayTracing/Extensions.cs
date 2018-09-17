using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public static bool IsNaN(this Complex c)
        {
            return double.IsNaN(c.Real) || double.IsNaN(c.Imaginary);
        }

        public static bool IsReal(this Complex a)
        {
            if (Math.Abs(a.Imaginary) < 0.0000001)
                return true;
            return false;
        }
    }
}
