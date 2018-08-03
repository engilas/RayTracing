using System;
using RayTracing.Models;
using RayTracing.Primitives;

namespace RayTracing {
	class Render {
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

			NormalizePlanes();

			for (int x = -xEdge + 1; x < xEdge; x++)
			for (int y = -yEdge + 1; y < yEdge; y++) {
				var D = CanvasToViewport(x, y);

				D = D.MultiplyMatrix(Helpers.GetXRotation(_options.CameraRotationX));
				D = D.MultiplyMatrix(Helpers.GetYRotation(_options.CameraRotationY));
				D = D.MultiplyMatrix(Helpers.GetZRotation(_options.CameraRotationZ));

				var color = TraceRay(_options.CameraPos, D, 1d, double.PositiveInfinity, _options.RecursionDepth);

				_canvas.DrawPoint(x, y, color.Clamp().ToColor());
			}
		}

		private Vector CanvasToViewport(double x, double y) {
			return new Vector(x * _options.ViewportWidth / _options.CanvasWidth,
				y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
		}

		private void NormalizePlanes() {
			foreach (var plane in _scene.Planes) {
				var a = IntersectRayPlane(_options.CameraPos, plane.Normal, plane);

				if (a > 0) {
					plane.D = -plane.D;
					plane.A = -plane.A;
					plane.B = -plane.B;
					plane.C = -plane.C;
				}
			}
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

			if (closestPrimitive is Box box) {
				normal = box.GetNormal(O, D, tMin);
			}

			normal = normal.Multiply(1 / normal.Lenght()); //unit vector

			var local_color = Vector.FromColor(closestPrimitive.Color)
				.Multiply(ComputeLighting(P, normal, view, closestPrimitive.Specular));

			var r = closestPrimitive.Reflect;
			if (depth <= 0 || r <= 0)
				return local_color;

			var R = ReflectRay(view, normal);
			var reflectedColor = TraceRay(P, R, 0.001d, double.PositiveInfinity, depth - 1);

			return local_color.Multiply(1 - r).Add(reflectedColor.Multiply(r));
		}

		private (Primitive, double) ClosestIntersection(Vector O, Vector D, double tMin, double tMax) {
			var closest_t = double.PositiveInfinity;
			Primitive closestPrimitive = null;
			foreach (var sphere in _scene.Spheres) {
				var (t1, t2) = IntersectRaySphere(O, D, sphere);
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
			var invertDir = D.Invert();
			foreach (var box in _scene.Boxes) {
				var t = IntersectRayBox(O, invertDir, box);
				if (t >= tMin && t <= tMax && t < closest_t) {
					closest_t = t;
					closestPrimitive = box;
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

			var t1 = (-k2 + Math.Sqrt(discr)) / (2 * k1);
			var t2 = (-k2 - Math.Sqrt(discr)) / (2 * k1);
			return (t1, t2);
		}

		private double IntersectRayPlane(Vector O, Vector D, Plane S) {
			var normal = S.Normal.DotProduct(D);

			if (Math.Abs(normal) > 0.00001)
				return -(S.A * O.D1 + S.B * O.D2 + S.C * O.D3 + S.D) / (S.A * D.D1 + S.B * D.D2 + S.C * D.D3);
			else return double.PositiveInfinity;
		}

		private double IntersectRayBox(Vector O, Vector D, Box box) {
			var lo = D.D1*(box.Min.D1 - O.D1);
			var hi = D.D1*(box.Max.D1 - O.D1);
			var tmin  = Math.Min(lo, hi);
			var tmax = Math.Max(lo, hi);
			
			var lo1 = D.D2*(box.Min.D2 - O.D2);
			var hi1 = D.D2*(box.Max.D2 - O.D2);
			tmin  = Math.Max(tmin, Math.Min(lo1, hi1));
			tmax = Math.Min(tmax, Math.Max(lo1, hi1));

			var lo2 = D.D3*(box.Min.D3 - O.D3);
			var hi2 = D.D3*(box.Max.D3 - O.D3);
			tmin  = Math.Max(tmin, Math.Min(lo2, hi2));
			tmax = Math.Min(tmax, Math.Max(lo2, hi2));

			if ((tmin <= tmax) && (tmax > 0)) {
				return tmin;
			} else 
				return double.PositiveInfinity;
		}

		private double ComputeLighting(Vector point, Vector normal, Vector view, int specular) {
			var i = 0.0;
			Vector L = null;
			foreach (var light in _scene.Lights) {
				if (light.Type == LightType.Ambient) {
					i += light.Intensity;
				} else {
					double tMax = 0;
					if (light.Type == LightType.Point) {
						L = light.Position.Subtract(point);
						tMax = 1;
					}
					if (light.Type == LightType.Direct) {
						L = light.Direction.Multiply(-1);
						tMax = double.PositiveInfinity;
					}

					var (shadow_sphere, _) = ClosestIntersection(point, L, 0.001, tMax);
					if (shadow_sphere != null)
						continue;

					var nDotL = normal.DotProduct(L);

					if (nDotL > 0) {
						var intensity = light.Intensity * nDotL / (normal.Lenght() * L.Lenght());
						i += intensity;
					}

					if (specular == -1) continue;

					var R = ReflectRay(L, normal);
					var rDotV = R.DotProduct(view);
					if (rDotV > 0) {
						var tmp = light.Intensity * Math.Pow(rDotV / (R.Lenght() * view.Lenght()), specular);
						i += (double) tmp;
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
