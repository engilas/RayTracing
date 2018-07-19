using System;

namespace DrawOpenGL.Models
{
	class Vector {
		public readonly float D1, D2, D3;
        
		public Vector((float, float, float) v) {
			(D1, D2, D3) = v;
		}

		public Vector(float v1, float v2, float v3) {
			D1 = v1;
			D2 = v2;
			D3 = v3;
		}

		public Vector Add(Vector v) {
			return new Vector(D1 + v.D1, D2 + v.D2, D3 + v.D3);
		}

		public Vector Multiply(float k) {
			return new Vector(k * D1, k * D2, k * D3);
		}

		public float Lenght() {
			return (float) Math.Sqrt(DotProduct(this));
		}

		public Vector Subtract(Vector v) {
			return new Vector(D1 - v.D1, D2 - v.D2, D3 - v.D3);
		}

		public float DotProduct(Vector v) {
			return D1 * v.D1 + D2 * v.D2 + D3 * v.D3;
		}
	}
}
