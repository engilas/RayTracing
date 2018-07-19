using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DrawOpenGL.Models;
using DrawOpenGL.Primitives;
using OpenTK;

namespace DrawOpenGL
{
	class Program {
		
        static void Main(string[] args)
        {
	        using (var canvasManager = new CanvasManager()) {

		        var width = 300;
		        var height = 300;
		        var bg = Color.Black;

		        canvasManager.Initialize(30,width,height, bg);
		        canvasManager.CancasClosed += (sender, eventArgs) =>  Environment.Exit(0);

		        var scene = new Scene {
			        Spheres = new List<Sphere>(new[] {
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
			        }),
			        Lights = new List<Light>(new[] {
				        new Light {
					        Type = LightType.Ambient,
					        Intensity = 0.2f
				        },
				        new Light {
					        Type = LightType.Point,
					        Intensity = 0.6f,
					        Position = new Vector(2, 1, 0)
				        },
				        new Light {
					        Type = LightType.Direct,
					        Intensity = 0.2f,
					        Direction = new Vector(1, 4, 4)
				        },
			        })
		        };

		        var options = new RenderOptions {
			        BgColor = bg,
			        CameraPos = new Vector(0, 0, 0),
			        ViewportWidth = 1,
			        ViewportHeight = 1,
			        CanvasWidth = width,
			        CanvasHeight = height,
			        ViewportDistance = 1
		        };

		        Console.WriteLine("Started");
		        
				var render = new Render(canvasManager.Canvas, scene, options);

		        var sw = Stopwatch.StartNew();
				render.Process();

				sw.Stop();

		        Console.WriteLine($"Rendered in {sw.Elapsed} ms");

				Thread.Sleep(-1);
	        }
        }

	    
    }

	
}
