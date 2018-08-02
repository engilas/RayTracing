using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    class Plane : Primitive
    {
	    public double A { get; set; }
		public double B { get; set; }
		public double C { get; set; }
		public double D { get; set; }

		//todo optimize dont create array, then replace abc to this
	    public Vector Normal => new Vector(A, B, C);
    }
}
