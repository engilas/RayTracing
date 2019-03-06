using System;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    internal class SurfaceCoeffs
    {
        public double
            A, //x2
            B, //y2
            C, //z2
            D, //z
            E, //y
            F; //const
    }

    internal class Surface : Primitive
    {
        public double
            A, //x2
            B, //y2
            C, //z2
            D, //z
            E, //y
            F; //const

        public double XMin, YMin, ZMin, XMax, YMax, ZMax;

        public Surface()
        {
            XMin = YMin = ZMin = double.MinValue;
            XMax = YMax = ZMax = double.MaxValue;
        }

        public Surface(SurfaceCoeffs coeffs) : this()
        {
            A = coeffs.A;
            B = coeffs.B;
            C = coeffs.C;
            D = coeffs.D;
            E = coeffs.E;
            F = coeffs.F;
        }

        public RotationMatrix Rotation { get; set; }
        public Vector Position { get; set; } = new Vector(0, 0, 0);

        public Vector GetNormal(Vector o, Vector d, double t)
        {
            o = new Vector(o.D1 - Position.D1, o.D2 - Position.D2, o.D3 - Position.D3);
            if (Rotation != null)
            {
                d = d.MultiplyMatrix(Rotation.Rotation);
                o = o.MultiplyMatrix(Rotation.Rotation);
            }

            var x = d.D1 * t + o.D1;
            var y = d.D2 * t + o.D2;
            var z = d.D3 * t + o.D3;

            var normal = new Vector(2 * A * x, 2 * B * y + E, 2 * C * z + D);

            if (Rotation != null)
                return normal.MultiplyMatrix(Rotation.RotationInv);
            return normal;
        }

        public static SurfaceCoeffs GetEllipsoid(double a, double b, double c)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                C = Math.Pow(c, -2),
                F = -1
            };
        }

        public static SurfaceCoeffs GetEllipticParaboloid(double a, double b)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                D = -1
            };
        }

        public static SurfaceCoeffs GetHyperbolicParaboloid(double a, double b)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = -Math.Pow(b, -2),
                D = -1
            };
        }

        public static SurfaceCoeffs GetEllipticHyperboloidOneSheet(double a, double b, double c)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                C = -Math.Pow(c, -2),
                F = -1
            };
        }

        public static SurfaceCoeffs GetEllipticHyperboloidTwoSheets(double a, double b, double c)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                C = -Math.Pow(c, -2),
                F = 1
            };
        }

        public static SurfaceCoeffs GetEllipticCone(double a, double b, double c)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                C = -Math.Pow(c, -2),
                F = 0
            };
        }

        public static SurfaceCoeffs GetEllipticCylinder(double a, double b)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = Math.Pow(b, -2),
                F = -1
            };
        }

        public static SurfaceCoeffs GetHyperbolicCylinder(double a, double b)
        {
            return new SurfaceCoeffs
            {
                A = Math.Pow(a, -2),
                B = -Math.Pow(b, -2),
                F = -1
            };
        }

        public static SurfaceCoeffs GetParabolicCylinder(double a)
        {
            return new SurfaceCoeffs
            {
                A = 1,
                E = 2 * a
            };
        }
    }
}