using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Sphere : Primitive {
		public void Init(Vector O)
	    {
	        Oc = O.Subtract(Center);

	        K3 = Oc.DotProduct(Oc) - Radius * Radius;
        }

        public Vector Oc { get; private set; }
	    public double K3 { get; private set; }
        public Vector Center { get; set; }
	    public double Radius { get; set; }
    }
}
