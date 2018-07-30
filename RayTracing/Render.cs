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
			    var color = TraceRay(_options.CameraPos, D, 1d, double.PositiveInfinity, _options.RecursionDepth);


			    _canvas.DrawPoint(x,y, color.Clamp().ToColor());
		    }
	    }

	    private Vector CanvasToViewport(double x, double y) {
		    return new Vector(x * _options.ViewportWidth / _options.CanvasWidth, y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
	    }

	    private Vector TraceRay(Vector O, Vector D, double tMin, double tMax, int depth) {

		    var (closestPrimitive, closest_t) = ClosestIntersection(O, D, tMin, tMax);
		    if (closestPrimitive == null) {
				return Vector.FromColor(_options.BgColor);
		    }

		    var view = D.Multiply(-1);
		    var P = O.Add(D.Multiply(closest_t));

		    Vector normal = null;

		    if (closestPrimitive is Plane plane) {
			    normal = plane.Normal;
		    }
		    
		    if (closestPrimitive is Sphere sphere) {
			    normal = P.Subtract(sphere.Center);
		    }

		    normal = normal.Multiply(1 / normal.Lenght());
		    var local_color = Vector.FromColor(closestPrimitive.Color)
			    .Multiply(ComputeLighting(P, normal, view, closestPrimitive.Specular, tMax));

		    var r = closestPrimitive.Reflect;
		    if (depth <= 0 || r <= 0)
			    return local_color;

		    var R = ReflectRay(view, normal);
		    var reflectedColor = TraceRay(P, R, 0.001d, double.PositiveInfinity, depth - 1);

		    return local_color.Multiply(1 - r).Add(reflectedColor.Multiply(r));
	    }

	    private (IPrimitive, double) ClosestIntersection(Vector O, Vector D, double tMin, double tMax) {
		    var closest_t = double.PositiveInfinity;
		    IPrimitive closestPrimitive = null;	
		    foreach (var sphere in _scene.Spheres) {
			    var(t1, t2) = IntersectRaySphere(O, D, sphere);
			    if (t1 >= tMin && t1 <= tMax && t1 < closest_t) {
				    closest_t = t1;
				    closestPrimitive = sphere;
			    }
			    if (t2 >= tMin && t2 <= tMax && t2 < closest_t) {
				    closest_t = t2;
				    closestPrimitive = sphere;
			    }
		    }
		    foreach (var plane in _scene.Planes) {
			    var t = IntersectRayPlane(O, D, plane);
			    if (t >= tMin && t <= tMax && t < closest_t) {
				    closest_t = t;
				    closestPrimitive = plane;
			    }
		    }
		    return (closestPrimitive, closest_t);
	    }

	    private (double, double) IntersectRaySphere(Vector O, Vector D, Sphere sphere) {
		    var C = sphere.Center;
		    var r = sphere.Radius;
		    var oc = O.Subtract(C);

		    var k1 = D.DotProduct(D);
		    var k2 = 2 * oc.DotProduct(D);
		    var k3 = oc.DotProduct(oc) - r * r;
		    var discr = k2 * k2 - 4 * k1 * k3;
		    if (discr < 0) {
			    return (double.PositiveInfinity, double.PositiveInfinity);
		    }

		    var t1 = (-k2 + (double)Math.Sqrt(discr)) / (2 * k1);
		    var t2 = (-k2 - (double)Math.Sqrt(discr)) / (2 * k1);
		    return (t1, t2);
	    }

	    private double IntersectRayPlane(Vector O, Vector D, Plane S) {

		    var normal = S.Normal.DotProduct(D);

		    if (Math.Abs(normal) > 0.00001)
			    return -(S.A * O.D1 + S.B * O.D2 + S.C * O.D3 + S.D) / (S.A * D.D1 + S.B * D.D2 + S.C * D.D3);
		    else return double.PositiveInfinity;

	    }

	    private double ComputeLighting(Vector P, Vector N, Vector V, int s, double tMax) {
		    var i = 0.0;
		    Vector L = null;
		    foreach (var light in _scene.Lights) {
			    if (light.Type == LightType.Ambient) {
				    i += light.Intensity;
			    } else {
				    if (light.Type == LightType.Point) {
					    L = light.Position.Subtract(P);
					    tMax = 1;
				    }
				    if (light.Type == LightType.Direct) {
					    L = light.Direction;
					    tMax = double.PositiveInfinity;
				    }

                    var (shadow_sphere, _) = ClosestIntersection(P, L, 0.001, tMax);
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
					    i += (double)tmp;
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
