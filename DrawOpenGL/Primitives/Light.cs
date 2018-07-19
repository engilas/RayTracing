using DrawOpenGL.Models;

namespace DrawOpenGL.Primitives {
	class Light {
		public LightType Type { get; set; }
		public float Intensity { get; set; }
		public Vector Position { get; set; }
		public Vector Direction { get; set; }
	}

	enum LightType {
		Ambient,
		Point,
		Direct
	}
}
