using System.Windows.Media;

namespace RayTracing.Primitives
{
    internal abstract class Primitive
    {
        public Color Color { get; set; }
        public int Specular { get; set; } = -1;
        public double Reflect { get; set; }
        public bool LightTransparent { get; set; }
    }
}