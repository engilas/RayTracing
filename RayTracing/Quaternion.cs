using System;
using RayTracing.Models;

namespace RayTracing
{
    internal class Quaternion
    {
        public readonly double w;
        public readonly double x;
        public readonly double y;
        public readonly double z;

        public Quaternion(Vector v, double rotationGrad)
        {
            double GetRad(double grad)
            {
                return Math.PI / 180 * grad;
            }

            var t = GetRad(rotationGrad) / 2;

            v = v.Multiply(1 / v.Lenght());

            w = Math.Cos(t);
            x = Math.Sin(t) * v.D1;
            y = Math.Sin(t) * v.D2;
            z = Math.Sin(t) * v.D3;
        }

        private Quaternion(double w, double x, double y, double z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Quaternion multiply(Quaternion q)
        {
            var qx = x * q.w + y * q.z - z * q.y + w * q.x;
            var qy = -x * q.z + y * q.w + z * q.x + w * q.y;
            var qz = x * q.y - y * q.x + z * q.w + w * q.z;
            var qw = -x * q.x - y * q.y - z * q.z + w * q.w;

            return new Quaternion(qw, qx, qy, qz);
        }

        private Quaternion conjugate()
        {
            return new Quaternion(w, -x, -y, -z);
        }

        private double norm()
        {
            return w * w + x * x + y * y + z * z;
        }

        //public static Quaternion normalise()
        //{
        //    double n = Math.Sqrt(norm());
        //    x /= n;
        //    y /= n;
        //    z /= n;
        //    w /= n;
        //}

        private Quaternion scale(double s)
        {
            return new Quaternion(w * s, x * s, y * s, z * s);
        }

        private Quaternion inverse()
        {
            return conjugate().scale(1 / norm());
        }

        public Vector Rotate(Vector v)
        {
            var vQ = new Quaternion(0, v.D1, v.D2, v.D3);
            var vS = multiply(vQ).multiply(inverse());
            return new Vector(vS.x, vS.y, vS.z);
        }
    }
}