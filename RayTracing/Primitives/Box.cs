using System;
using System.Linq;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Box : Primitive {
		public Vector Min { get; set; }
		public Vector Max { get; set; }

		class t {
			public double Value;
			public Axis Axis;
			public bool Negative;
		}

		enum Axis {
			x,y,z
		}

		public Vector GetNormal(Vector O, Vector D, double tMin)
		{
		   //todo: optimize (t value also computes in the render class)
			D = D.Invert();

			var xlo = D.D1 * (Min.D1 - O.D1);
			var xhi = D.D1 * (Max.D1 - O.D1);
			var txmin = xlo < xhi
				? new t {Axis = Axis.x, Value = xlo, Negative = true}
				: new t {Axis = Axis.x, Value = xhi, Negative = false};

			var ylo = D.D2 * (Min.D2 - O.D2);
			var yhi = D.D2 * (Max.D2 - O.D2);
			var tymin = ylo < yhi
				? new t {Axis = Axis.y, Value = ylo, Negative = true}
				: new t {Axis = Axis.y, Value = yhi, Negative = false};

			var zlo = D.D3 * (Min.D3 - O.D3);
			var zhi = D.D3 * (Max.D3 - O.D3);
			var tzmin = zlo < zhi
				? new t {Axis = Axis.z, Value = zlo, Negative = true}
				: new t {Axis = Axis.z, Value = zhi, Negative = false};
            
		    var min = txmin;
		    if (tymin.Value > min.Value)
		        min = tymin;
		    if (tzmin.Value > min.Value)
		        min = tzmin;
			
			switch (min.Axis) {
				case Axis.x: return new Vector(min.Negative ? -1 : 1, 0, 0);
				case Axis.y: return new Vector(0, min.Negative ? -1 : 1, 0);
				case Axis.z: return new Vector(0, 0, min.Negative ? -1 : 1);
				default: throw new Exception();
			}
		}
	}
}
