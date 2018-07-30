namespace RayTracing.Models {
	class Pixel {
		public double X { get; }
		public double Y { get; }
		public Vector Color { get; }

		public Pixel(double x, double y, Vector c) {
			X = x;
			Y = y;
			Color = c;
		}
	}
}
