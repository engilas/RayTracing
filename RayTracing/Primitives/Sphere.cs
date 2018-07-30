using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Sphere : IPrimitive {
		public Vector Center { get; set; }
		public double Radius { get; set; }
		public Color Color { get; set; }
		public int Specular { get; set; }
		public double Reflect { get; set; }
	}
}
