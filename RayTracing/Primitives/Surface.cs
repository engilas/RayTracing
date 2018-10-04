using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    class Surface : Primitive {
	    public double A, B, C, D, E, F;

	    public RotationMatrix Rotation { get; set; }
	    public Vector Position { get; set; } = new Vector(0,0,0);

        public Vector GetNormal(Vector o, Vector d, double t)
        {
	        o = new Vector(o.D1 - Position.D1, o.D2 - Position.D2, o.D3 - Position.D3);
	        if (Rotation != null) {
		        d = d.MultiplyMatrix(Rotation.Rotation);
		        o = o.MultiplyMatrix(Rotation.Rotation);
	        }

            var x = d.D1 * t + o.D1;
            var y = d.D2 * t + o.D2;
            var z = d.D3 * t + o.D3;

	        var normal = new Vector(2 * A * x, 2 * B * y + E, 2 * C * z + D);

	        if (Rotation != null) {
		        return normal.MultiplyMatrix(Rotation.RotationInv);
	        } else {
		        return normal;
	        }
        }
    }
}
