using RayTracing.Models;

namespace RayTracing.Primitives
{
    internal class Sphere : Primitive
    {
        public Vector Center { get; set; }
        public double Radius { get; set; }
    }
}