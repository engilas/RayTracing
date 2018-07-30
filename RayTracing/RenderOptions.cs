using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing
{
    class RenderOptions {
	    public int CanvasWidth { get; set; } 
	    public int CanvasHeight { get; set; } 

	    public int ViewportWidth{ get; set; }
	    public int ViewportHeight{ get; set; } 

		public int RecursionDepth { get; set; }

	    public Color BgColor{ get; set; } 

	    public Vector CameraPos { get; set; }
		public double[,] CameraRotation { get; set; }

	    public int ViewportDistance{ get; set; }
    }
}
