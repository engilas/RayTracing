using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    class Plane : IPrimitive
    {
		public Color Color { get; set; }
	    public int Specular { get; set; }
	    public float Reflect { get; set; }

	    public float A { get; set; }
		public float B { get; set; }
		public float C { get; set; }
		public float D { get; set; }

		//todo optimize dont create array, then replace abc to this
	    public Vector Normal => new Vector(A, B, C);
    }
}
