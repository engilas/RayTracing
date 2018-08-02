using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Box : Primitive {
		public Vector Min { get; set; }
		public Vector Max { get; set; }
	}
}
