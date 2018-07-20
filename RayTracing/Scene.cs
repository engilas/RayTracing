using System.Collections.Generic;
using RayTracing.Primitives;

namespace RayTracing
{
    class Scene
    {
		public List<Sphere> Spheres { get; set; }
		public List<Light> Lights { get; set; }
    }
}
