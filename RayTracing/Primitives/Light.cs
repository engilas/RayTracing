using RayTracing.Models;

namespace RayTracing.Primitives {
	class Light {
		public LightType Type { get; set; }
		public double Intensity { get; set; }
		public Vector Position { get; set; }
		public Vector Direction { get; set; }
	}

	enum LightType {
		Ambient,
		Point,
		Direct
	}
}
