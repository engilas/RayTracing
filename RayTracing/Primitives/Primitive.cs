using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	abstract class Primitive {
		public Color Color { get; set; }
	    public int Specular { get; set; } = -1;
		public double Reflect { get; set; }
	    public bool LightTransparent { get; set; }
    }
}
