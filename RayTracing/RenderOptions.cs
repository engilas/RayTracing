using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing
{
    class RenderOptions {
	    public int CanvasWidth { get; set; } 
	    public int CanvasHeight { get; set; } 

	    public double ViewportWidth{ get; set; }
	    public double ViewportHeight{ get; set; } 

		public int RecursionDepth { get; set; }

	    public Color BgColor{ get; set; } 

	    public Vector CameraPos { get; set; }

		public double CameraRotationX { get; set; }
	    public double CameraRotationY { get; set; }
	    public double CameraRotationZ { get; set; }

	    public double ViewportDistance{ get; set; }
    }
}
