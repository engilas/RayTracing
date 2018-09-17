using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using RayTracing.Models;
using RayTracing.Primitives;
using Plane = RayTracing.Primitives.Plane;

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
		    var pixelCount = _options.CanvasWidth * _options.CanvasHeight;
            
            InitPrimitives();

		    var sw = Stopwatch.StartNew();

            var rotationMtx = new RotationMatrix(_options.CameraRotationX, _options.CameraRotationY, _options.CameraRotationZ);

            var result = new Color[pixelCount];
		    Parallel.For(0, pixelCount, i =>
		            //for (int i = 0; i < pixelCount; i++)
		        {
		            var x = i % _options.CanvasWidth - xEdge;
		            var y = i / _options.CanvasHeight - yEdge;

		            var D = CanvasToViewport(x, y);

		            D = D.MultiplyMatrix(rotationMtx.X);
		            D = D.MultiplyMatrix(rotationMtx.Y);
		            D = D.MultiplyMatrix(rotationMtx.Z);

		            var color = TraceRay(_options.CameraPos, D, 1d, double.PositiveInfinity, _options.RecursionDepth);

		            result[i] = color.Clamp().ToColor();
		        }
		    );

            for (int i = 0; i < pixelCount; i++)
            {
                var x = i % _options.CanvasWidth - xEdge;
                var y = i / _options.CanvasHeight - yEdge;
                _canvas.DrawPoint(x, y, result[i]);
            }

            sw.Stop();
            Console.WriteLine(sw.Elapsed);

			
		}

		private Vector CanvasToViewport(double x, double y) {
			return new Vector(x * _options.ViewportWidth / _options.CanvasWidth,
				y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
		}

		private void InitPrimitives() {
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
			    if (sphere.LightTransparent)
			    {
			        return Vector.FromColor(sphere.Color);
			    }
				normal = P.Subtract(sphere.Center);
            }

			if (closestPrimitive is Box box) {
				normal = box.GetNormal(O, D, tMin);
            }
            if (closestPrimitive is Surface surface)
            {
                if (surface.Torus)
                {
                    return Vector.FromColor(Color.FromRgb(255,255,255));
                }

                normal = surface.GetNormal(O, D, closest_t);
            }

		    normal = normal.Multiply(1 / normal.Lenght()); //unit vector

		    Color color = closestPrimitive.Color;
		    if (closestPrimitive is Plane)
		    {
		        var x = (int) Math.Round(O.D1 + D.D1 * closest_t) % 2;
		        var z = (int) Math.Round(O.D3 + D.D3 * closest_t) % 2;
                if (x == z)
                    color = Color.FromRgb(0,0,0);

            }

            var local_color = Vector.FromColor(color)
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
		    foreach (var surface in _scene.Surfaces)
		    {
		        var t = IntersectRayParaboloid(surface, O, D);
		        if (!double.IsNaN(t))
		        {
		            if (t >= tMin && t <= tMax && t < closest_t)
		            {
		                closest_t = t;
		                closestPrimitive = surface;
		            }
                }
            }
		    {
		        var t = IntersectRayTorus(O, D);
		        if (!double.IsNaN(t))
		        {
		            if (t >= tMin && t <= tMax && t < closest_t)
		            {
		                closest_t = t;
		                closestPrimitive = new Surface {Torus = true};
		            }
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
		    if (discr < 0)
		    {
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

	    private double IntersectRayParaboloid(Surface paraboloid, Vector O, Vector D)
	    {
	        var o = O;
	        var d = D;

	        if (paraboloid.Offset != null)
	        {
	            o = new Vector(o.D1 - paraboloid.Offset.D1, o.D2 - paraboloid.Offset.D2, o.D3 - paraboloid.Offset.D3);
	        }

            if (paraboloid.AxisDirection == Axis.Z)
	        {
	            o = new Vector(o.D1, o.D2, o.D3);
	            d = new Vector(d.D1, d.D2, d.D3);
            }
            else if (paraboloid.AxisDirection == Axis.Y)
	        {
	            o = new Vector(o.D1, o.D3, o.D2);
                d = new Vector(d.D1, d.D3, d.D2);
	        }
	        else if (paraboloid.AxisDirection == Axis.X)
	        {
	            o = new Vector(o.D3, o.D2, o.D1);
	            d = new Vector(d.D3, d.D2, d.D1);
	        } else throw new Exception($"Unknown direction {paraboloid.AxisDirection}");

	        int dirMultiplier = 1;
	        if (paraboloid.Direction == Direction.Down)
	        {
	            dirMultiplier = -1;
	        }
	        
            var width = paraboloid.Width;

            //x^2+y^2+wz=0
            //var p1 = dirMultiplier * 1 / (2 * (Pow2(d.D1) + Pow2(d.D2)));
            //var p2 = dirMultiplier * (-2 * d.D1 * o.D1 - 2 * d.D2 * o.D2) + width * d.D3;
            //var p3 = Math.Sqrt(Pow2(dirMultiplier * (2 * d.D1 * o.D1 + 2 * d.D2 * o.D2) - width * d.D3) -
            //                   4 * (dirMultiplier * (Pow2(d.D1) + Pow2(d.D2))) *
            //                   (dirMultiplier * (Pow2(o.D1) + Pow2(o.D2)) - width * o.D3));//cache this line

	        var p1 = 1 / (2 * (Pow2(d.D1) - Pow2(d.D2)));
	        var p2 = (-2 * d.D1 * o.D1 + 2 * d.D2 * o.D2) + width * d.D3;
	        var p3 = Math.Sqrt(Pow2((2 * d.D1 * o.D1 - 2 * d.D2 * o.D2) - width * d.D3) -
	                           4 * ((Pow2(d.D1) - Pow2(d.D2))) *
	                           ((Pow2(o.D1) - Pow2(o.D2)) - width * o.D3));//cache this line

            var t1 = p1 * (p2 - p3);
            var t2 = p1 * (p2 + p3);

            //edge by direction
            var tMin = Math.Min(t1, t2);
	        var tMax = Math.Max(t1, t2);
	        if (paraboloid.Edge > 0 && Math.Abs(dirMultiplier * (d.D3 * tMin + o.D3)) > paraboloid.Edge)
	        {
	            if (Math.Abs(dirMultiplier * (d.D3 * tMax + o.D3) )> paraboloid.Edge)
	                return double.PositiveInfinity;
	            else return tMax;
	        }
	        if (paraboloid.Edge > 0 && Math.Abs(dirMultiplier * (d.D2 * tMin + o.D2)) > paraboloid.Edge)
	        {
	            if (Math.Abs(dirMultiplier * (d.D2 * tMax + o.D2)) > paraboloid.Edge)
	                return double.PositiveInfinity;
	            else return tMax;
	        }
	        if (paraboloid.Edge > 0 && Math.Abs(dirMultiplier * (d.D1 * tMin + o.D1)) > paraboloid.Edge)
	        {
	            if (Math.Abs(dirMultiplier * (d.D1 * tMax + o.D1)) > paraboloid.Edge)
	                return double.PositiveInfinity;
	            else return tMax;
	        }

            return tMin;
	    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    private double Pow2(double val)
        {
            return val * val;
        }

	    private double IntersectRayTorus(Vector O, Vector D)
	    {
	        var o = O;
	        var d = D;

	        var r = 0.4d;
	        var R = 1d;

	        var ox = O.D1;
	        var oy = O.D2;
	        var oz = O.D3;

	        var dx = D.D1;
	        var dy = D.D2;
	        var dz = D.D3;

	        // define the coefficients of the quartic equation
	        var sum_d_sqrd = dx * dx + dy * dy + dz * dz;
	        var e = ox * ox + oy * oy + oz * oz -
	                R * R - r * r;
	        var f = ox * dx + oy * dy + oz * dz;
	        var four_a_sqrd = 4.0 * R * R;

	        var coeffs = new [] {
	            e * e - four_a_sqrd * (r * r - oy * oy),
	            4.0 * f * e + 2.0 * four_a_sqrd * oy * dy,
	            2.0 * sum_d_sqrd * e + 4.0 * f * f + four_a_sqrd * dy * dy,
	            4.0 * sum_d_sqrd * f,
	            sum_d_sqrd* sum_d_sqrd
	        }.Reverse().ToArray();

            List<Complex> solve = new List<Complex>();

            try
            {
                solve = RealPolynomialRootFinder.FindRoots(coeffs);
            }
            catch
            {
                return double.PositiveInfinity;
            }

            double min = double.PositiveInfinity;

            for (int i = 0; i < solve.Count; i++)
            {
                if (solve[i].Real > 0.0001 && Math.Abs(solve[i].Imaginary) < 0.0001 && solve[i].Real < min)
                {
                    min = solve[i].Real;
                }
            }

            //var solve1 = RealPolynomialRootFinder.SolveQuartic(coeffs[0], coeffs[1], coeffs[2], coeffs[3], coeffs[4]);

            //for (int i = 0; i < solve1.Length; i++)
            //{
            //    if (/*solve1[i] > 0.0001 &&*/ solve1[i] < min)
            //    {
            //        min = solve1[i];
            //    }
            //}

            return min;
	    }



        /*
  (p1                 )    (p2                    )   (p3     )  (p4                                                                       )   (p5           
 -(d1*o1 + d2*o2 + d3*o3 + (- d1^2*o2^2 - d1^2*o3^2 + d1^2*r^2 - 2*R*d1^2 + 2*d1*d2*o1*o2 + 2*d1*d3*o1*o3 - d2^2*o1^2 - d2^2*o3^2 + d2^2*r^2 - 2*R*d2^2 + 
 
                                                  )   (p6    )          (p7               )         
   2*d2*d3*o2*o3 - d3^2*o1^2 - d3^2*o2^2 + d3^2*r^2 - 2*R*d3^2)^(1/2))/(d1^2 + d2^2 + d3^2)

 -(d1*o1 + d2*o2 + d3*o3 + (- d1^2*o2^2 - d1^2*o3^2 + d1^2*r^2 + 2*R*d1^2 + 2*d1*d2*o1*o2 + 2*d1*d3*o1*o3 - d2^2*o1^2 - d2^2*o3^2 + d2^2*r^2 + 2*R*d2^2 + 2*d2*d3*o2*o3 - d3^2*o1^2 - d3^2*o2^2 + d3^2*r^2 + 2*R*d3^2)^(1/2))/(d1^2 + d2^2 + d3^2)

 -(d1*o1 + d2*o2 + d3*o3 - (- d1^2*o2^2 - d1^2*o3^2 + d1^2*r^2 - 2*R*d1^2 + 2*d1*d2*o1*o2 + 2*d1*d3*o1*o3 - d2^2*o1^2 - d2^2*o3^2 + d2^2*r^2 - 2*R*d2^2 + 2*d2*d3*o2*o3 - d3^2*o1^2 - d3^2*o2^2 + d3^2*r^2 - 2*R*d3^2)^(1/2))/(d1^2 + d2^2 + d3^2)

 -(d1*o1 + d2*o2 + d3*o3 - (- d1^2*o2^2 - d1^2*o3^2 + d1^2*r^2 + 2*R*d1^2 + 2*d1*d2*o1*o2 + 2*d1*d3*o1*o3 - d2^2*o1^2 - d2^2*o3^2 + d2^2*r^2 + 2*R*d2^2 + 2*d2*d3*o2*o3 - d3^2*o1^2 - d3^2*o2^2 + d3^2*r^2 + 2*R*d3^2)^(1/2))/(d1^2 + d2^2 + d3^2)

        */

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

					var (shadowPrimitive, _) = ClosestIntersection(point, L, 0.001, tMax);
					if (shadowPrimitive != null && !shadowPrimitive.LightTransparent)
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

		private Vector ReflectRay(Vector r, Vector normal) {
			return normal.Multiply(2 * r.DotProduct(normal)).Subtract(r);
		}

	}
}
