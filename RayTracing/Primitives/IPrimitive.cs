using System.Windows.Media;

namespace RayTracing.Primitives
{
    interface IPrimitive
    {
	    Color Color { get; }
	    int Specular { get; }
	    float Reflect { get; }
    }
}
