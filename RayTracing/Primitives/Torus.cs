using RayTracing.Models;

namespace RayTracing.Primitives
{
    internal class Torus : Primitive
    {
        public readonly Box AroundBox;
        public readonly double SweptRadius;
        public readonly double TubeRadius;

        public Torus(double tubeRadius, double sweptRadius)
        {
            TubeRadius = tubeRadius;
            SweptRadius = sweptRadius;

            var boxWidth = sweptRadius + tubeRadius;
            var boxHeight = tubeRadius;

            AroundBox = new Box
            {
                Min = new Vector(-boxWidth, -boxHeight, -boxWidth),
                Max = new Vector(boxWidth, boxHeight, boxWidth)
            };
        }

        public RotationMatrix Rotation { get; set; }
        public Vector Position { get; set; }

        public Vector GetNormal(Vector o, Vector d, double t)
        {
            o = new Vector(o.D1 - Position.D1, o.D2 - Position.D2, o.D3 - Position.D3);
            if (Rotation != null)
            {
                d = d.MultiplyMatrix(Rotation.Rotation);
                o = o.MultiplyMatrix(Rotation.Rotation);
            }

            var x = o.D1 + d.D1 * t;
            var y = o.D2 + d.D2 * t;
            var z = o.D3 + d.D3 * t;

            var paramSquared = SweptRadius * SweptRadius + TubeRadius * TubeRadius;

            var sumSquared = x * x + y * y + z * z;

            var normal = new Vector(
                4.0 * x * (sumSquared - paramSquared),
                4.0 * y * (sumSquared - paramSquared + 2.0 * SweptRadius * SweptRadius),
                4.0 * z * (sumSquared - paramSquared));

            if (Rotation != null)
                return normal.MultiplyMatrix(Rotation.RotationInv);
            return normal;
        }
    }
}