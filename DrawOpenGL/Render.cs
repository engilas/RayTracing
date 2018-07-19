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
			    _canvas.DrawPoint(x,y, color);
		    }
	    }

	    private Vector CanvasToViewport(float x, float y) {
		    return new Vector(x * _options.ViewportWidth / _options.CanvasWidth, y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
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

		    var P = O.Add(D.Multiply(closest_t));
		    var N = P.Subtract(closest_sphere.Center);
		    N = N.Multiply(1 / N.Lenght());
		    var lightning = ComputeLighting(P, N, D.Multiply(-1), closest_sphere.Specular);
		    return Multiply(lightning, closest_sphere.Color);
	    }

	    private (float, float) IntersectRaySphere(Vector O, Vector D, Sphere sphere) {
		    var C = sphere.Center;
		    var r = sphere.Radius;
		    var oc = O.Subtract(C);

		    var k1 = D.DotProduct(D);
		    var k2 = 2 * oc.DotProduct(D);
		    var k3 = oc.DotProduct(oc) - r * r;
		    var discr = k2 * k2 - 4 * k1 * k3;
		    if (discr < 0) {
			    return (float.PositiveInfinity, float.PositiveInfinity);
		    }

		    var t1 = (-k2 + (float)Math.Sqrt(discr)) / (2 * k1);
		    var t2 = (-k2 - (float)Math.Sqrt(discr)) / (2 * k1);
		    return (t1, t2);
	    }
		
	    private Color Multiply(float k, Color color) {
		    var r = (int) (k * color.R);
			var g = (int) (k * color.G);
		    var b = (int) (k * color.B);

			int normalize(int c) => c > 255 ? 255 : c < 0 ? 0 : c;

		    r = normalize(r);
		    g = normalize(g);
		    b = normalize(b);

		    return new Color(r, g, b, 255);
	    }

	    private float ComputeLighting(Vector P, Vector N, Vector V, int s) {
		    var i = 0.0f;
		    Vector L = null;
		    foreach (var light in _scene.Lights) {
			    if (light.Type == LightType.Ambient) {
				    i += light.Intensity;
			    } else {
				    if (light.Type == LightType.Point) {
					    L = light.Position.Subtract(P);
				    }
				    if (light.Type == LightType.Direct) {
					    L = light.Direction;
				    }

				    var nDotL = N.DotProduct(L);

				    if (nDotL > 0) {
					    i += light.Intensity * nDotL / (N.Lenght() * L.Lenght());
				    }

				    if (s == -1) continue;

				    var R = N.Multiply(2).Multiply(N.DotProduct(L)).Subtract(L);
				    var rDotV = R.DotProduct(V);
				    if (rDotV > 0) {
						var tmp = light.Intensity * Math.Pow(rDotV / (R.Lenght() * V.Lenght()), s);
					    i += (float)tmp;
				    }
			    }
		    }
		    return i;
	    }

		/// <summary>
        /// Clamps a color to the canonical color range.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
	    private Color Clamp(Color v) {
			return new Color(Math.Min(255, (int) Math.Max(0d, v.R)),
				(int) Math.Min(255, Math.Max(0d, v.G)),
				(int) Math.Min(255, Math.Max(0d, v.B)), 255);
		}

    }
}
