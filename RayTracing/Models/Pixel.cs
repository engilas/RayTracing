namespace RayTracing.Models {
	class Pixel {
		public float X { get; }
		public float Y { get; }
		public Vector Color { get; }

		public Pixel(float x, float y, Vector c) {
			X = x;
			Y = y;
			Color = c;
		}
	}
}
