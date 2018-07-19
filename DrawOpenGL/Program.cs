using System;
using System.Collections.Generic;
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

		        var width = 200;
		        var height = 200;
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
			        })
		        };

		        var options = new RenderOptions {
			        BgColor = Color.Black,
			        CameraPos = new Vector(0, 0, 0),
			        ViewportWidth = 1,
			        ViewportHeight = 1,
			        CanvasWidth = 200,
			        CanvasHeight = 200,
			        ViewportDistance = 1
		        };

				var render = new Render(canvasManager.Canvas, scene, options);
				render.Process();


				Console.WriteLine("Started");
				Thread.Sleep(-1);
	        }
        }

	    
    }

	
}
