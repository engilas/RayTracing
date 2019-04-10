using RayTracing.Models;

namespace RayTracing.Primitives
{
    internal class Disk : Primitive
    {
        public readonly double A;
        public readonly double B;
        public readonly Vector Normal = new Vector(0, 0, 1);
        public readonly double R;
        public readonly double R2;

        public Disk(double a, double b, double r)
        {
            A = a;
            B = b;
            R = r;
            R2 = r * r;
        }

        public RotationMatrix Rotation { get; set; }
        public Vector Position { get; set; } = new Vector(0, 0, 0);

        public Vector GetNormal()
        {
            if (Rotation != null)
                return Normal.MultiplyMatrix(Rotation.RotationInv);
            return Normal;
        }
    }
}