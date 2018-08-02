using System.Collections.Generic;
using RayTracing.Primitives;

namespace RayTracing
{
    class Scene
    {
		public List<Sphere> Spheres { get; set; } = new List<Sphere>();
		public List<Light> Lights { get; set; } = new List<Light>();
		public List<Plane> Planes { get; set; } = new List<Plane>();
	    public List<Box> Boxes { get; set; } = new List<Box>();
    }
}
