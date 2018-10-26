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
        public List<Surface> Surfaces { get; set; } = new List<Surface>();
	    public List<Torus> Toruses { get; set; } = new List<Torus>();
        public List<Disk> Disks { get; set; } = new List<Disk>();
    }
}
