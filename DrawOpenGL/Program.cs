using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DrawOpenGL
{
	struct Vector {
		public readonly float D1, D2, D3;
        
		public Vector((float, float, float) v) {
			(D1, D2, D3) = v;
		}

		public Vector(float v1, float v2, float v3) {
			D1 = v1;
			D2 = v2;
			D3 = v3;
		}
	}

	class Sphere {
		public Vector Center { get; set; }
		public float Radius { get; set; }
		public Color Color { get; set; }
	}

    class Program {
	    static int cW = 600;
	    static int cH = 600;

	    static int vW = 1;
	    static int vH = 1;

	    private static Color bgColor = Color.Black;

	    private static List<Sphere> spheres = new List<Sphere>();

	    private static Vector O = new Vector(0, 0, 0);

	    private static int d = 1;

        static void Main(string[] args)
        {
	        using (var canvasManager = new CanvasManager()) {
		        
		        canvasManager.Initialize(30,cW,cH, bgColor);

				spheres.AddRange(
					new [] {
						new Sphere {
							Center = new Vector(0, -1, 3),
							Radius = 1,
							Color = Color.Red
						},
						new Sphere {
							Center = new Vector(2, 0, 4),
							Radius = 1,
							Color = Color.Blue
						},
						new Sphere {
							Center = new Vector(-2, 0, 4),
							Radius = 1,
							Color = Color.Green
						}
					}
					);

				Console.WriteLine("Started");

		        for (int x = (int)Math.Round(-cW/2d); x < Math.Round(cW/2d); x++)
		        for (int y = (int)Math.Round(-cH/2d); y < Math.Round(cH/2d); y++) {
					var D = CanvasToViewport(x, y);
			        var color = TraceRay(O, D, 1, int.MaxValue);
					canvasManager.Canvas.DrawPoint(x,y,color);
		        }
				

				Thread.Sleep(-1);
	        }
        }

	    static Vector CanvasToViewport(float x, float y) {
		    return new Vector(x * vW / cW, y * vH / cH, d);
	    }

	    static float DotProduct(Vector v1, Vector v2) {
		    return v1.D1 * v2.D1 + v1.D2 * v2.D2 + v1.D3 * v2.D3;
	    }

	    static Vector Subtract(Vector v1, Vector v2) {
            return new Vector(v1.D1 - v2.D1, v1.D2 - v2.D2, v1.D3 - v2.D3);
	    }

	    static Color TraceRay(Vector O, Vector D, int tMin, int tMax) {
		    var closest_t = float.PositiveInfinity;
		    Sphere closest_sphere = null;
		    foreach (var sphere in spheres) {
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
			    return bgColor;
		    return closest_sphere.Color;
	    }

	    static (float, float) IntersectRaySphere(Vector O, Vector D, Sphere sphere) {
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
    }

	
}
