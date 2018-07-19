﻿using System.Collections.Generic;
using DrawOpenGL.Primitives;

namespace DrawOpenGL
{
    class Scene
    {
		public List<Sphere> Spheres { get; set; }
		public List<Light> Lights { get; set; }
    }
}
