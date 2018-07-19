﻿using DrawOpenGL.Models;
using OpenTK;

namespace DrawOpenGL
{
    class RenderOptions {
	    public int CanvasWidth { get; set; } 
	    public int CanvasHeight { get; set; } 

	    public int ViewportWidth{ get; set; }
	    public int ViewportHeight{ get; set; } 

	    public Color BgColor{ get; set; } 

	    public Vector CameraPos { get; set; }

	    public int ViewportDistance{ get; set; }
    }
}