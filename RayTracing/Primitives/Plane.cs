using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives
{
    class Plane : Primitive {
	    public double A;
	    public double B;
	    public double C;
	    public double D;

	    public Vector Normal;

	    public Plane(double a, double b, double c, double d) {
		    AssignCoeffs(a,b,c,d);
	    }

	    public void AssignCoeffs(double a, double b, double c, double d) {
		    A = a;
		    B = b;
		    C = c;
		    D = d;

		    Normal = new Vector(A, B, C);
	    }
    }
}
