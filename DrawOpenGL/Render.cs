using System;
using DrawOpenGL.Models;
using DrawOpenGL.Primitives;
using OpenTK;

namespace DrawOpenGL
{
    class Render
    {
	    private readonly ICanvas _canvas;
	    private readonly Scene _scene;
	    private readonly RenderOptions _options;

	    public Render(ICanvas canvas, Scene scene, RenderOptions options) {
		    _canvas = canvas;
		    _scene = scene;
		    _options = options;
	    }

	    public void Process() {
		    var xEdge = (int) Math.Round(_options.CanvasWidth / 2d);
			var yEdge = (int) Math.Round(_options.CanvasHeight / 2d);

		    for (int x = -xEdge; x < xEdge; x++)
		    for (int y = -yEdge; y < yEdge; y++) {
			    var D = CanvasToViewport(x, y);
			    var color = TraceRay(_options.CameraPos, D, 1, int.MaxValue);
			    _canvas.DrawPoint(x,y,color);
		    }
	    }

	    private Vector CanvasToViewport(float x, float y) {
		    return new Vector(x * _options.ViewportWidth / _options.CanvasWidth, y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
	    }

	    static float DotProduct(Vector v1, Vector v2) {
		    return v1.D1 * v2.D1 + v1.D2 * v2.D2 + v1.D3 * v2.D3;
	    }

	    static Vector Subtract(Vector v1, Vector v2) {
		    return new Vector(v1.D1 - v2.D1, v1.D2 - v2.D2, v1.D3 - v2.D3);
	    }

	    private Color TraceRay(Vector O, Vector D, int tMin, int tMax) {
		    var closest_t = float.PositiveInfinity;
		    Sphere closest_sphere = null;
		    foreach (var sphere in _scene.Spheres) {
			    var(t1, t2) = IntersectRaySphere(O, D, sphere);
			    if (t1 >= tMin && t1 <= tMax && t1 < closest_t) {
				    closest_t = t1;
				    closest_sphere = sphere;
			    }
			    if (t2 >= tMin && t2 <= tMax && t2 < closest_t) {
				    closest_t = t2;
				    closest_sphere = sphere;
			    }
		    }
		    if (closest_sphere == null)
			    return _options.BgColor;

		    var P = Add(O, Multiply(closest_t, D));
		    var N = Subtract(P, closest_sphere.Center);
		    N = Multiply(1 / Lenght(N), N);

		    return Multiply(ComputeLighting(P, N), closest_sphere.Color);
	    }

	    private (float, float) IntersectRaySphere(Vector O, Vector D, Sphere sphere) {
		    var C = sphere.Center;
		    var r = sphere.Radius;
		    var oc = Subtract(O, C);

		    var k1 = DotProduct(D, D);
		    var k2 = 2 * DotProduct(oc, D);
		    var k3 = DotProduct(oc, oc) - r * r;
		    var discr = k2 * k2 - 4 * k1 * k3;
		    if (discr < 0) {
			    return (float.PositiveInfinity, float.PositiveInfinity);
		    }

		    var t1 = (-k2 + (float)Math.Sqrt(discr)) / (2 * k1);
		    var t2 = (-k2 - (float)Math.Sqrt(discr)) / (2 * k1);
		    return (t1, t2);
	    }

	    private float Lenght(Vector v) {
		    return (float) Math.Sqrt(DotProduct(v, v));
	    }

	    private Vector Multiply(float k, Vector v) {
			return new Vector(k * v.D1, k * v.D2, k * v.D3);
	    }
		
	    private Color Multiply(float k, Color c) {
		    return new Color((int) (k * c.R), (int) (k * c.G), (int) (k * c.B), 255);
	    }

	    private Vector Add(Vector v1, Vector v2) {
			return new Vector(v1.D1 + v2.D1, v1.D2 + v2.D2, v1.D3 + v2.D3);
	    }

	    private float ComputeLighting(Vector P, Vector N) {
		    var i = 0.0f;
		    Vector L = null;
		    foreach (var light in _scene.Lights) {
			    if (light.Type == LightType.Ambient) {
				    i += light.Intensity;
			    } else {
				    if (light.Type == LightType.Point) {
					    L = Subtract(light.Position, P);
				    }
				    if (light.Type == LightType.Direct) {
					    L = light.Direction;
				    }

				    var nDotL = DotProduct(N, L);

				    if (nDotL > 0) {
					    i += light.Intensity * nDotL / (Lenght(N) * Lenght(L));
				    }
			    }
		    }
		    return i;
	    }

    }
}
