using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    class Surface : Primitive
    {
        public Vector GetNormal(Vector o, Vector d, double t)
        {
            var x = d.D1 * t + o.D1;
            var y = d.D2 * t + o.D2;
            var z = d.D3 * t + o.D3;

            return new Vector(2 * x, 2 * y, -z)/*.Multiply(-1)*/;
        }
    }
}
