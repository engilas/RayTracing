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

    class Surface : Primitive {
	    public double A, B, C, D, E, F;

        public Axis AxisDirection { get; set; }
        public double Width { get; set; }
        public double Edge { get; set; }
        public Vector Offset { get; set; }

        public Direction Direction { get; set; }

        public Vector GetNormal(Vector o, Vector d, double t)
        {
            var x = d.D1 * t + o.D1;
            var y = d.D2 * t + o.D2;
            var z = d.D3 * t + o.D3;

            return new Vector(2 * A * x, 2 * B  * y + E, 2 * C * z + D);
        }
    }
}
