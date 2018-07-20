using System;
using RayTracing.Models;
using RayTracing.Primitives;

namespace RayTracing
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

		    for (int x = -xEdge + 1; x < xEdge; x++)
		    for (int y = -yEdge + 1; y < yEdge; y++) {
			    var D = CanvasToViewport(x, y);
			    if (_options.CameraRotation != null)
				    D = D.MultiplyMatrix(_options.CameraRotation);
			    var color = TraceRay(_options.CameraPos, D, 1, int.MaxValue, _options.RecursionDepth);


			    _canvas.DrawPoint(x,y, color.Clamp().ToColor());
		    }
	    }

	    private Vector CanvasToViewport(float x, float y) {
		    return new Vector(x * _options.ViewportWidth / _options.CanvasWidth, y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
	    }

	    private Vector TraceRay(Vector O, Vector D, float tMin, float tMax, int depth) {
		    var (closest_sphere, closest_t) = ClosestIntersection(O, D, tMin, tMax);

		    if (closest_sphere == null)
			    return Vector.FromColor(_options.BgColor);

		    var view = D.Multiply(-1);

		    var P = O.Add(D.Multiply(closest_t));
		    var N = P.Subtract(closest_sphere.Center);
		    N = N.Multiply(1 / N.Lenght());
		    var local_color = Vector.FromColor(closest_sphere.Color)
			    .Multiply(ComputeLighting(P, N, view, closest_sphere.Specular, tMax));
			
            var r = closest_sphere.Reflect;
            if (depth <= 0 || r <= 0)
                return local_color;

            var R = ReflectRay(D.Multiply(-1), N);
            var reflectedColor = TraceRay(P, R, 0.1f, float.PositiveInfinity, depth - 1);

            return local_color.Multiply(1 - r).Add(reflectedColor.Multiply(r));
        }

	    private (Sphere, float) ClosestIntersection(Vector O, Vector D, float tMin, float tMax) {
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
		    return (closest_sphere, closest_t);
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

	    private float ComputeLighting(Vector P, Vector N, Vector V, int s, float tMax) {
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

                    var (shadow_sphere, _) = ClosestIntersection(P, L, 0.001f, tMax);
                    if (shadow_sphere != null)
                        continue;

                    var nDotL = N.DotProduct(L);

				    if (nDotL > 0) {
					    i += light.Intensity * nDotL / (N.Lenght() * L.Lenght());
				    }

				    if (s == -1) continue;

				    var R = ReflectRay(L, N);
				    var rDotV = R.DotProduct(V);
				    if (rDotV > 0) {
						var tmp = light.Intensity * Math.Pow(rDotV / (R.Lenght() * V.Lenght()), s);
					    i += (float)tmp;
				    }
			    }
		    }
		    return i;
	    }

	    private Vector ReflectRay(Vector v1, Vector v2) {
		    return v2.Multiply(2 * v1.DotProduct(v2)).Subtract(v1);
	    }

    }
}
