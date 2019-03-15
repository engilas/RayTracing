using System;
using System.Windows.Media;

namespace RayTracing.Models
{
    internal class Vector
    {
        public readonly double D1, D2, D3;

        public Vector((double, double, double) v)
        {
            (D1, D2, D3) = v;
        }

        public Vector(double v1, double v2, double v3)
        {
            D1 = v1;
            D2 = v2;
            D3 = v3;
        }

        public Vector Add(Vector v)
        {
            return new Vector(D1 + v.D1, D2 + v.D2, D3 + v.D3);
        }

        public Vector Multiply(double k)
        {
            return new Vector(k * D1, k * D2, k * D3);
        }

        public Vector MultiplyMatrix(double[,] matrix)
        {
            var result = new[] {0d, 0d, 0d};
            var vec = new[] {D1, D2, D3};
            for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                result[i] += vec[j] * matrix[i, j];
            return new Vector(result[0], result[1], result[2]);
        }

        public double Lenght()
        {
            return Math.Sqrt(DotProduct(this));
        }

        public Vector Subtract(Vector v)
        {
            return new Vector(D1 - v.D1, D2 - v.D2, D3 - v.D3);
        }

        public double DotProduct(Vector v)
        {
            return D1 * v.D1 + D2 * v.D2 + D3 * v.D3;
        }

        public Color ToColor()
        {
            return Color.FromRgb((byte) D1, (byte) D2, (byte) D3);
        }

        /// <summary>
        ///     Clamps a color to the canonical color range.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector Clamp()
        {
            return new Vector(Math.Min(255, (int) Math.Max(0d, D1)),
                (int) Math.Min(255, Math.Max(0d, D2)),
                (int) Math.Min(255, Math.Max(0d, D3)));
        }

        public static Vector FromColor(Color c)
        {
            return new Vector(c.R, c.G, c.B);
        }

        public Vector Invert()
        {
            return new Vector(1 / D1, 1 / D2, 1 / D3);
        }

        public Vector Normalize() => this.Multiply(1 / this.Lenght());
    }
}