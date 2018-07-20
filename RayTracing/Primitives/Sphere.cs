using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Sphere {
		public Vector Center { get; set; }
		public float Radius { get; set; }
		public Color Color { get; set; }
		public int Specular { get; set; }
		public float Reflect { get; set; }
	}
}
