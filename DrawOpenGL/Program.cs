using System;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DrawOpenGL
{
    class Program
    {
        static void Main(string[] args)
        {
	        using (var canvasManager = new CanvasManager()) {
		        int w, h;
		        w = h = 600;
		        canvasManager.Initialize(30,w,h);

				Console.WriteLine("Started");

		        for (int i = -w; i <= w; i++) {
			        Thread.Sleep(1);
			        canvasManager.Canvas.DrawPoint(i, i, Color.Aqua);
			        canvasManager.Canvas.DrawPoint(-i, i, Color.Aqua);
			        canvasManager.Canvas.DrawPoint(0, -i, Color.Aqua);
			        canvasManager.Canvas.DrawPoint(-i, 0, Color.Aqua);
		        }
				

				Thread.Sleep(-1);
	        }
        }
    }

	
}
