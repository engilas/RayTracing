using DrawOpenGL.Models;
using OpenTK;

namespace DrawOpenGL.Primitives {
	class Sphere {
		public Vector Center { get; set; }
		public float Radius { get; set; }
		public Color Color { get; set; }
		public int Specular { get; set; }
	}
}
