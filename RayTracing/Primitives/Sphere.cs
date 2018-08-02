using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Sphere : Primitive {
		public Vector Center { get; set; }
		public double Radius { get; set; }
	}
}
