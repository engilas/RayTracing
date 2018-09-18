using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public enum Direction { Up, Down }

    class Surface : Primitive
    {
        public Axis AxisDirection { get; set; }
        public double Width { get; set; }
        public double Edge { get; set; }
        public Vector Offset { get; set; }

        public bool Torus { get; set; }

        public Direction Direction { get; set; }

        public Vector GetNormal(Vector o, Vector d, double t)
        {
            if (Torus)
                return TorusNormal(o, d, t);

            int dirMultiplier = 1;
            if (Direction == Direction.Down)
            {
                dirMultiplier = -1;
            }
            if (Offset != null)
            {
                o = new Vector(o.D1 - Offset.D1, o.D2 - Offset.D2, o.D3 - Offset.D3);
            }

            var a = AxisDirection == Axis.X ? -1 : dirMultiplier * 2 * (d.D1 * t + o.D1);
            var b = AxisDirection == Axis.Y ? -1 : dirMultiplier * 2 * (d.D2 * t + o.D2);
            var c = AxisDirection == Axis.Z ? -1 : dirMultiplier * 2 * (d.D3 * t + o.D3);
            //var x = d.D1 * t + o.D1;
            //var y = d.D2 * t + o.D2;

            return new Vector(a, -b, c).Multiply(dirMultiplier);
        }

        private Vector TorusNormal(Vector o, Vector d, double t)
        {
            var x = o.D1 + d.D1 * t;
            var y = o.D2 + d.D2 * t;
            var z = o.D3 + d.D3 * t;

            var paramSquared = 1 * 1 + 0.4 * 0.4;

            var sumSquared = x * x + y * y + z * z;

            var tmp = new Vector(
                4.0 * x * (sumSquared - paramSquared),
                4.0 * y * (sumSquared - paramSquared + 2.0 * 1 * 1),
                4.0 * z * (sumSquared - paramSquared));

            return tmp;
        }
    }
}
