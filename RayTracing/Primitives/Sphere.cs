using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	class Sphere : Primitive {
		public Vector Oc { get; private set; }
	    public double K3 { get; private set; }
        public Vector Center { get; set; }
	    public double Radius { get; set; }
    }
}
