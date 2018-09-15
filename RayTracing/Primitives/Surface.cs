using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    public enum Direction
    {
        X,
        Y,
        Z
    }

    class Surface : Primitive
    {
        public Direction Direction { get; set; }
        public double Width { get; set; }
        public double Edge { get; set; }

        public Vector GetNormal(Vector o, Vector d, double t)
        {
            var a = Direction == Direction.X ? -1 : 2 * (d.D1 * t + o.D1);
            var b = Direction == Direction.Y ? -1 : 2 * (d.D2 * t + o.D2);
            var c = Direction == Direction.Z ? -1 : 2 * (d.D3 * t + o.D3);
            //var x = d.D1 * t + o.D1;
            //var y = d.D2 * t + o.D2;

            return new Vector(a, b, c);
        }
    }
}
