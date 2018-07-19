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
	}
}
