﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using RayTracing.Models;
using RayTracing.Primitives;

namespace RayTracing
{
    internal class Render
    {
        private readonly ICanvas _canvas;
        private readonly RenderOptions _options;
        private readonly Scene _scene;

        public Render(ICanvas canvas, Scene scene, RenderOptions options)
        {
            _canvas = canvas;
            _scene = scene;
            _options = options;
        }

        public void Process()
        {
            var xEdge = (int) Math.Round(_options.CanvasWidth / 2d);
            var yEdge = (int) Math.Round(_options.CanvasHeight / 2d);
            var pixelCount = _options.CanvasWidth * _options.CanvasHeight;

            var sw = Stopwatch.StartNew();

            var rotationMtx = new RotationMatrix(_options.CameraRotationX, _options.CameraRotationY,
                _options.CameraRotationZ);

            var result = new Color[pixelCount];
            Parallel.For(0, pixelCount, i =>
                    //        for (int i = 0; i < pixelCount; i++)
                {
                    var x = i % _options.CanvasWidth - xEdge;
                    var y = i / _options.CanvasHeight - yEdge;

                    var D = CanvasToViewport(x, y);

                    D = D.MultiplyMatrix(rotationMtx.Rotation);

                    var color = TraceRay(_options.CameraPos, D, 1d, double.PositiveInfinity, _options.RecursionDepth);

                    result[i] = color.Clamp().ToColor();
                }
            );

            for (var i = 0; i < pixelCount; i++)
            {
                var x = i % _options.CanvasWidth - xEdge;
                var y = i / _options.CanvasHeight - yEdge;
                _canvas.DrawPoint(x, y, result[i]);
            }

            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }

        private Vector CanvasToViewport(double x, double y)
        {
            return new Vector(x * _options.ViewportWidth / _options.CanvasWidth,
                y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
        }

        private Vector TraceRay(Vector O, Vector D, double tMin, double tMax, int depth)
        {
            var (closestPrimitive, closest_t) = ClosestIntersection(O, D, tMin, tMax);
            if (closestPrimitive == null) return Vector.FromColor(_options.BgColor);

            var view = D.Multiply(-1);
            var P = O.Add(D.Multiply(closest_t));

            Vector normal = new Vector();

            if (closestPrimitive is Plane plane) normal = plane.Normal;

            if (closestPrimitive is Sphere sphere)
            {
                if (sphere.LightTransparent) return Vector.FromColor(sphere.Color);
                normal = P.Subtract(sphere.Center);
            }

            if (closestPrimitive is Box box) normal = box.GetNormal(O, D, tMin);
            if (closestPrimitive is Surface surface) normal = surface.GetNormal(O, D, closest_t);
            if (closestPrimitive is Torus torus) normal = torus.GetNormal(O, D, closest_t);

            if (closestPrimitive is Disk disk) normal = disk.GetNormal();

            normal = normal.Multiply(1 / normal.Lenght()); //unit vector
            if (normal.DotProduct(D) > 0) normal = normal.Multiply(-1);

            var color = closestPrimitive.Color;
            if (closestPrimitive is Plane)
            {
                var x = (int) Math.Round(O.D1 + D.D1 * closest_t) % 2;
                var z = (int) Math.Round(O.D3 + D.D3 * closest_t) % 2;
                if (x == z)
                    color = Color.FromRgb(0, 0, 0);
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

        private (Primitive, double) ClosestIntersection(Vector O, Vector D, double tMin, double tMax)
        {
            var closest_t = double.PositiveInfinity;
            Primitive closestPrimitive = null;
            foreach (var sphere in _scene.Spheres)
            {
                var (t1, t2) = IntersectRaySphere(O, D, sphere);
                if (t1 >= tMin && t1 <= tMax && t1 < closest_t)
                {
                    closest_t = t1;
                    closestPrimitive = sphere;
                }

                if (t2 >= tMin && t2 <= tMax && t2 < closest_t)
                {
                    closest_t = t2;
                    closestPrimitive = sphere;
                }
            }

            foreach (var plane in _scene.Planes)
            {
                var t = IntersectRayPlane(O, D, plane);
                if (t >= tMin && t <= tMax && t < closest_t)
                {
                    closest_t = t;
                    closestPrimitive = plane;
                }
            }

            var invertDir = D.Invert();
            foreach (var box in _scene.Boxes)
            {
                var t = IntersectRayBox(O, invertDir, box);
                if (t >= tMin && t <= tMax && t < closest_t)
                {
                    closest_t = t;
                    closestPrimitive = box;
                }
            }

            foreach (var surface in _scene.Surfaces)
            {
                var t = IntersectRaySurface(surface, O, D);
                if (!double.IsNaN(t))
                    if (t >= tMin && t <= tMax && t < closest_t)
                    {
                        closest_t = t;
                        closestPrimitive = surface;
                    }
            }

            foreach (var torus in _scene.Toruses)
            {
                var t = IntersectRayTorus(torus, O, D);
                if (!double.IsNaN(t))
                    if (t >= tMin && t <= tMax && t < closest_t)
                    {
                        closest_t = t;
                        closestPrimitive = torus;
                    }
            }

            foreach (var disk in _scene.Disks)
            {
                var t = IntersectRayDisk(disk, O, D);
                if (!double.IsNaN(t))
                    if (t >= tMin && t <= tMax && t < closest_t)
                    {
                        closest_t = t;
                        closestPrimitive = disk;
                    }
            }

            return (closestPrimitive, closest_t);
        }

        private (double, double) IntersectRaySphere(Vector O, Vector D, Sphere sphere)
        {
            var C = sphere.Center;
            var r = sphere.Radius;
            var oc = O.Subtract(C);

            var k1 = D.DotProduct(D);
            var k2 = 2 * oc.DotProduct(D);
            var k3 = oc.DotProduct(oc) - r * r;
            var discr = k2 * k2 - 4 * k1 * k3;
            if (discr < 0) return (double.PositiveInfinity, double.PositiveInfinity);

            var t1 = (-k2 + Math.Sqrt(discr)) / (2 * k1);
            var t2 = (-k2 - Math.Sqrt(discr)) / (2 * k1);
            return (t1, t2);
        }

        private double IntersectRayPlane(Vector O, Vector D, Plane S)
        {
            var normal = S.Normal.DotProduct(D);

            if (Math.Abs(normal) > 0.00001)
                return -(S.A * O.D1 + S.B * O.D2 + S.C * O.D3 + S.D) / (S.A * D.D1 + S.B * D.D2 + S.C * D.D3);
            return double.PositiveInfinity;
        }

        private double IntersectRayBox(Vector O, Vector D, Box box)
        {
            var lo = D.D1 * (box.Min.D1 - O.D1);
            var hi = D.D1 * (box.Max.D1 - O.D1);
            var tmin = Math.Min(lo, hi);
            var tmax = Math.Max(lo, hi);

            var lo1 = D.D2 * (box.Min.D2 - O.D2);
            var hi1 = D.D2 * (box.Max.D2 - O.D2);
            tmin = Math.Max(tmin, Math.Min(lo1, hi1));
            tmax = Math.Min(tmax, Math.Max(lo1, hi1));

            var lo2 = D.D3 * (box.Min.D3 - O.D3);
            var hi2 = D.D3 * (box.Max.D3 - O.D3);
            tmin = Math.Max(tmin, Math.Min(lo2, hi2));
            tmax = Math.Min(tmax, Math.Max(lo2, hi2));

            if (tmin <= tmax && tmax > 0)
                return tmin;
            return double.PositiveInfinity;
        }

        private double IntersectRaySurface(Surface surface, Vector O, Vector D)
        {
            var origO = new Vector(O);
            var origD = new Vector(D);

            var a = surface.A;
            var b = surface.B;
            var c = surface.C;
            var d = surface.D;
            var e = surface.E;
            var f = surface.F;

            O = new Vector(O.D1 - surface.Position.D1, O.D2 - surface.Position.D2, O.D3 - surface.Position.D3);

            if (surface.Rotation != null)
            {
                D = D.MultiplyMatrix(surface.Rotation.Rotation);
                O = O.MultiplyMatrix(surface.Rotation.Rotation);
            }

            var d1 = D.D1;
            var d2 = D.D2;
            var d3 = D.D3;
            var o1 = O.D1;
            var o2 = O.D2;
            var o3 = O.D3;

            var p1 = 2 * a * d1 * o1 + 2 * b * d2 * o2 + 2 * c * d3 * o3 + d * d3 + d2 * e;
            var p2 = a * d1.Pow2() + b * d2.Pow2() + c * d3.Pow2();
            var p3 = a * o1.Pow2() + b * o2.Pow2() + c * o3.Pow2() + d * o3 + e * o2 + f;
            var p4 = Math.Sqrt(p1.Pow2() - 4 * p2 * p3);

            //division by zero
            if (Math.Abs(p2) < 1e-20)
            {
                var t = -p3 / p1;
                return t;
            }

            var min = double.PositiveInfinity;
            var max = double.PositiveInfinity;

            var t1 = (-p1 - p4) / (2 * p2);
            var t2 = (-p1 + p4) / (2 * p2);

            var epsilon = 1e-4;

            if (t1 > epsilon && t1 < min)
            {
                min = t1;
                max = t2;
            }

            if (t2 > epsilon && t2 < min)
            {
                min = t2;
                max = t1;
            }

            var vMin = new Vector(surface.XMin, surface.YMin, surface.ZMin);
            var vMax = new Vector(surface.XMax, surface.YMax, surface.ZMax);

            if (!CheckSurfaceEdges(origD, origO, ref min, ref max, vMin, vMax, epsilon))
                return double.PositiveInfinity;

            return min;
        }

        private bool CheckSurfaceEdges(Vector d, Vector o, ref double tMin, ref double tMax, Vector min, Vector max, double epsilon)
        {
            var pt = d.Multiply(tMin).Add(o);
            if (!pt.Between(min, max))
            {
                if (tMax < epsilon) return false;

                pt = d.Multiply(tMax).Add(o);

                if (!pt.Between(min, max)) return false;

                Swap(ref tMin, ref tMax);
            }

            return true;
        }

        private void Swap(ref double d1, ref double d2)
        {
            var tmp = d1;
            d1 = d2;
            d2 = tmp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Pow2(double val)
        {
            return val * val;
        }

        private double IntersectRayTorus(Torus torus, Vector O, Vector D)
        {
            var o = O;
            var d = D;

            var r = torus.TubeRadius;
            var R = torus.SweptRadius;

            o = new Vector(o.D1 - torus.Position.D1, o.D2 - torus.Position.D2, o.D3 - torus.Position.D3);

            if (torus.Rotation != null)
            {
                d = d.MultiplyMatrix(torus.Rotation.Rotation);
                o = o.MultiplyMatrix(torus.Rotation.Rotation);
            }

            var intersectBox = IntersectRayBox(o, d.Invert(), torus.AroundBox);

            if (double.IsNaN(intersectBox) || double.IsPositiveInfinity(intersectBox)) return double.NaN;

            var ox = o.D1;
            var oy = o.D2;
            var oz = o.D3;

            var dx = d.D1;
            var dy = d.D2;
            var dz = d.D3;

            var rPow2 = r * r;
            var RPow2 = R * R;
            var dyPow2 = dy * dy;
            var oyPow2 = oy * oy;

            // define the coefficients of the quartic equation
            var sum_d_sqrd = dx * dx + dyPow2 + dz * dz;
            var e = ox * ox + oyPow2 + oz * oz -
                    RPow2 - rPow2;
            var f = ox * dx + oy * dy + oz * dz;
            var four_a_sqrd = 4.0 * RPow2;

            var coeffs = new[]
            {
                sum_d_sqrd * sum_d_sqrd,
                4.0 * sum_d_sqrd * f,
                2.0 * sum_d_sqrd * e + 4.0 * f * f + four_a_sqrd * dyPow2,
                4.0 * f * e + 2.0 * four_a_sqrd * oy * dy,
                e * e - four_a_sqrd * (rPow2 - oyPow2)
            };

            var solve = RealPolynomialRootFinder.FindRoots(coeffs);
            if (solve == null)
                return double.PositiveInfinity;

            var min = double.PositiveInfinity;

            for (var i = 0; i < solve.Count; i++)
                if (solve[i].IsReal() && solve[i].Real > 0.0001 && solve[i].Real < min)
                    min = solve[i].Real;

            return min;
        }

        private double IntersectRayDisk(Disk disk, Vector O, Vector D)
        {
            O = new Vector(O.D1 - disk.Position.D1, O.D2 - disk.Position.D2, O.D3 - disk.Position.D3);
            if (disk.Rotation != null)
            {
                D = D.MultiplyMatrix(disk.Rotation.Rotation);
                O = O.MultiplyMatrix(disk.Rotation.Rotation);
            }

            var t = -O.D3 / D.D3;

            var x = D.D1 * t + O.D1;
            var y = D.D2 * t + O.D2;

            if (x * x / disk.A + y * y / disk.B < disk.R2) return t;

            return double.PositiveInfinity;
        }

        private double ComputeLighting(Vector point, Vector normal, Vector view, int specular)
        {
            var i = 0.0;
            Vector L = new Vector();
            foreach (var light in _scene.Lights)
                if (light.Type == LightType.Ambient)
                {
                    i += light.Intensity;
                }
                else
                {
                    double tMax = 0;
                    if (light.Type == LightType.Point)
                    {
                        L = light.Position.Subtract(point);
                        tMax = 1;
                    }

                    if (light.Type == LightType.Direct)
                    {
                        L = light.Direction.Multiply(-1);
                        tMax = double.PositiveInfinity;
                    }

                    var (shadowPrimitive, _) = ClosestIntersection(point, L, 0.001, tMax);
                    if (shadowPrimitive != null && !shadowPrimitive.LightTransparent)
                        continue;

                    var nDotL = normal.DotProduct(L);

                    if (nDotL > 0)
                    {
                        var intensity = light.Intensity * nDotL / (normal.Lenght() * L.Lenght());
                        i += intensity;
                    }

                    if (specular == -1) continue;

                    var R = ReflectRay(L, normal);
                    var rDotV = R.DotProduct(view);
                    if (rDotV > 0)
                    {
                        var tmp = light.Intensity * Math.Pow(rDotV / (R.Lenght() * view.Lenght()), specular);
                        i += tmp;
                    }
                }

            return i;
        }

        private Vector ReflectRay(Vector r, Vector normal)
        {
            return normal.Multiply(2 * r.DotProduct(normal)).Subtract(r);
        }
    }
}