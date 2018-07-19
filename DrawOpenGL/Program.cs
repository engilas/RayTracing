﻿using System;
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
					        Color = Color.Red,
					        Specular = 500
				        },
				        new Sphere {
					        Center = new Vector(2, 0, 4),
					        Radius = 1,
					        Color = Color.Blue,
					        Specular = 500
				        },
				        new Sphere {
					        Center = new Vector(-2, 0, 4),
					        Radius = 1,
					        Color = Color.Green,
					        Specular = 10
				        },
				        new Sphere {
					        Center = new Vector(0, -5001, 0),
					        Radius = 5000f,
					        Color = Color.Yellow,
					        Specular = 600
				        }
			        }),
			        Lights = new List<Light>(
				        new[] {
                            new Light {
                                Type = LightType.Ambient,
                                Intensity = 0.2f
                            },
					        //new Light {
						       // Type = LightType.Point,
						       // Intensity = 0.6f,
						       // Position = new Vector(2, 1, 0)
					        //},
					        new Light {
						        Type = LightType.Direct,
						        Intensity = 0.99f,
						        Direction = new Vector(1, 4, 4)
					        }
                        }
			        )
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

		        ProcessRender(render);

		        canvasManager.Resize += (sender, eventArgs) => {
					canvasManager.Canvas.Clear();
			        options.CanvasWidth = canvasManager.Canvas.Width;
			        options.CanvasHeight = canvasManager.Canvas.Height;
			        ProcessRender(render);
		        };

				Thread.Sleep(-1);
	        }
        }

		static void ProcessRender(Render render) {
			

			render.Process();
			var sw = Stopwatch.StartNew();
			render.Process();

			sw.Stop();

			Console.WriteLine($"Rendered in {sw.Elapsed} ms");
		}
	    
    }

	
}
