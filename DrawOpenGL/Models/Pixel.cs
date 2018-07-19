using OpenTK;

namespace DrawOpenGL.Models {
	class Pixel {
		public float X { get; }
		public float Y { get; }
		public Color Color { get; }

		public Pixel(float x, float y, Color c) {
			X = x;
			Y = y;
			Color = c;
		}
	}
}
