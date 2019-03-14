using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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

            //var rotationMtx = new RotationMatrix(45, -45, 0);
            var quat = new Quaternion(new Vector(0, 1, 0), _options.CameraRotationY)
                .multiply(new Quaternion(new Vector(1, 0, 0), _options.CameraRotationX));

            var result = new Color[pixelCount];
            //Parallel.For(0, pixelCount, i =>
                    for (int i = 0; i < pixelCount; i++)
                {
                    var x = i % _options.CanvasWidth - xEdge;
                    var y = i / _options.CanvasHeight - yEdge;

                    var D = CanvasToViewport(x, y);

                    if (x == 0 && y == 0)
                    {
                    }

                    D = quat.Rotate(D);

                    var color = TraceRay(_options.CameraPos, D, 1d, double.PositiveInfinity,
                        _options.RecursionDepth);

                    result[i] = color.Clamp().ToColor();
                }
            //);

            for (var i = 0; i < pixelCount; i++)
            {
                var x = i % _options.CanvasWidth - xEdge;
                var y = i / _options.CanvasHeight - yEdge;
                _canvas.DrawPoint(x, y, result[i]);
            }

            var hash = CalcHash(result);

            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.WriteLine(hash);
        }

        private string CalcHash(Color[] colors)
        {
            var bytes = colors.SelectMany(x => new[] {x.R, x.G, x.B}).ToArray();
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(bytes);
                var sb = new StringBuilder();
                for (var i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                return sb.ToString();
            }
        }

        private Vector CanvasToViewport(double x, double y)
        {
            return new Vector(x * _options.ViewportWidth / _options.CanvasWidth,
                y * _options.ViewportHeight / _options.CanvasHeight, _options.ViewportDistance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        double clamp(double v, double lo, double hi) 
        { return Math.Max(lo, Math.Min(hi, v)); }

        Vector refract(Vector I, Vector N, double ior)
        {
            double cosi = clamp(I.DotProduct(N), -1, 1);
            double etai = 1, etat = ior;
            Vector n = N.Multiply(1);
            if (cosi < 0) { cosi = -cosi; }
            else
            {
                double tmp = etai;
                etai = etat;
                etat = tmp;
                //swap(etai, etat);
                n = N.Multiply(-1);
            }
            double eta = etai / etat;
            double k = 1 - eta * eta * (1 - cosi * cosi);
            return k < 0.0 ? new Vector(0,0,0) : I.Multiply(eta).Add(n.Multiply(eta * cosi - Math.Sqrt(k)));
        }

        void fresnel(Vector I, Vector N, double ior, out double kr)
        {
            double cosi = clamp(I.DotProduct(N), -1, 1);
            double etai = 1, etat = ior;
            if (cosi > 0)
            {
                double tmp = etai;
                etai = etat;
                etat = tmp;
                //swap(etai, etat); 
            }
            // Compute sini using Snell's law
            double sint = etai / etat * Math.Sqrt(Math.Max(0.0, 1 - cosi * cosi));
            // Total internal reflection
            if (sint >= 1)
            {
                kr = 1;
            }
            else
            {
                double cost = Math.Sqrt(Math.Max(0.0, 1 - sint * sint));
                cosi = Math.Abs(cosi);
                double Rs = ((etat * cosi) - (etai * cost)) / ((etat * cosi) + (etai * cost));
                double Rp = ((etai * cosi) - (etat * cost)) / ((etai * cosi) + (etat * cost));
                kr = (Rs * Rs + Rp * Rp) / 2;
            }
        }

        private Vector TraceRay(Vector O, Vector D, double tMin, double tMax, int depth, bool noRefract = false)
        {
            var maxRecDepth = 6;

            var colors = new Vector[maxRecDepth];
            var reflects = new double[maxRecDepth];
            var krs = new double[maxRecDepth];
            var refracts = new Vector[maxRecDepth];

            var recursionCount = 0;

            void SetResult(Vector color, double refl)
            {
                colors[recursionCount] = color;
                reflects[recursionCount] = refl;
                ++recursionCount;
            }

            for (var i = 0; i < depth; i++)
            {
                var (closestPrimitive, closest_t) = ClosestIntersection(O, D, tMin, tMax);
                if (closestPrimitive == null)
                {
                    SetResult(Vector.FromColor(_options.BgColor), 0);
                    break;
                }

                var view = D.Multiply(-1);
                var P = O.Add(D.Multiply(closest_t));

                Vector normal = null;

                if (closestPrimitive is Plane plane) normal = plane.Normal;

                if (closestPrimitive is Sphere sphere)
                {
                    if (sphere.LightTransparent)
                    {
                        SetResult(Vector.FromColor(sphere.Color), 0);
                        break;
                    }

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

                //refract
                if (closestPrimitive.Refract > 0 && !noRefract)
                {
                    bool outside = D.DotProduct(normal) < 0;
                    Vector bias = normal.Multiply(0.001);
                    double kr;
                    //fresnel(D, normal, closestPrimitive.Refract, out kr);

                    //if (outside)
                    //{
                    //    Vector reflectionDirection = ReflectRay(view, normal);
                    //    Vector reflectionRayOrig = P.Add(bias);
                    //    Vector reflectionColor;
                    //    reflectionColor = TraceRay(reflectionRayOrig, reflectionDirection, tMin, tMax, 1, true);
                    //    //if (TraceRayOneHit(reflectionRayOrig, reflectionDirection, tMin, tMax, reflectionColor))
                    //    //{
                    //    refracts[recursionCount] = reflectionColor;// [recursionCount] = reflectionColor;
                    //    //    isRefract[recursionCount] = true;
                    //    krs[recursionCount] = kr;
                    //    //}
                    //}

                    //if (kr < 1)
                    {
                        --i;
                        Vector refractionDirection = refract(D, normal, closestPrimitive.Refract).Multiply(1 / normal.Lenght());
                        Vector refractionRayOrig =  P.Add(bias.Multiply(outside ? -1 : 1));
                        //refractionColor = castRay(refractionRayOrig, refractionDirection, objects, lights, options, depth + 1); 
                        O = refractionRayOrig;
                        D = refractionDirection;
                        //break;
                    }
                    //else
                    //{
                    //    return new Vector(1, 1, 1);
                    //}
                }
                else
                {
                    var local_color = Vector.FromColor(color)
                        .Multiply(
                            ComputeLighting(P, normal, view, closestPrimitive.Specular));

                    //if last reflected ray
                    //if (i == depth - 1)
                    //{
                    //    normals.Add
                    //}
                    var r = closestPrimitive.Reflect;

                    SetResult(local_color, r);

                    if (r <= 0 || depth == 1)
                        break;

                    //setup for next iteration
                    O = P;
                    D = ReflectRay(view, normal);
                    ;
                    tMin = 0.001d;
                    tMax = double.PositiveInfinity;
                }
                //

                


                //var reflectedColor = TraceRay(P, R, 0.001d, double.PositiveInfinity, depth - 1);

                //return local_color.Multiply(1 - r).Add(reflectedColor.Multiply(r));
            }

            if (recursionCount <= 1)
                return colors[0];

            var totalColor = colors[recursionCount - 1];

            for (int i = recursionCount - 2; i >= 0; i--)
            {
                double reflect = reflects[i];
                var prevColor = colors[i];
                totalColor = prevColor.Multiply(1 - reflect).Add(totalColor.Multiply(reflect));
                //if (isRefract[i])
                    //return vec4(1, 0, 0, 0);
                //totalColor = totalColor * krs[i] + refracts[i] * (1 - krs[i]);
            }

            return totalColor;

            //var colorsCount = colors.Length;
            //var totalColor = colors.Last();
            //if (depth == 1 || colorsCount == 1)
            //    return totalColor;

            //for (var i = colors.Length - 2; i >= 0; i--)
            //{
            //    var reflect = reflects[i];
            //    var prevColor = colors[i];
            //    totalColor = prevColor.Multiply(1 - reflect).Add(totalColor.Multiply(reflect));
            //}

            //return totalColor;
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

            if (!CheckSurfaceEdges(D.D1, O.D1, ref min, ref max, surface.XMin,
                surface.XMax, epsilon))
                return double.PositiveInfinity;

            if (!CheckSurfaceEdges(D.D2, O.D2, ref min, ref max, surface.YMin,
                surface.YMax, epsilon))
                return double.PositiveInfinity;

            if (!CheckSurfaceEdges(D.D3, O.D3, ref min, ref max, surface.ZMin,
                surface.ZMax, epsilon))
                return double.PositiveInfinity;

            return min;
        }

        private bool CheckSurfaceEdges(double d, double o, ref double tMin, ref double tMax, double axisMin,
            double axisMax, double epsilon)
        {
            var axisValue = d * tMin + o;
            if (axisValue > axisMax || axisValue < axisMin)
            {
                if (tMax < epsilon) return false;

                axisValue = d * tMax + o;

                if (axisValue > axisMax || axisValue < axisMin) return false;

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
            Vector L = null;
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