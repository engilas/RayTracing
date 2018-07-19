using DrawOpenGL.Models;
using OpenTK;

namespace DrawOpenGL
{
    class RenderOptions {
	    public int CanvasWidth { get; set; } = 200;
	    public int CanvasHeight { get; set; } = 200;

	    public int ViewportWidth{ get; set; } = 1;
	    public int ViewportHeight{ get; set; } = 1;

	    public Color BgColor{ get; set; } = Color.Black;

	    public Vector CameraPos { get; set; } = new Vector(0, 0, 0);

	    public int ViewportDistance{ get; set; } = 1;
    }
}
