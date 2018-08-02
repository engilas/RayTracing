using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	abstract class Primitive {
		public Color Color { get; set; }
		public int Specular { get; set; }
		public double Reflect { get; set; }
	}
}
