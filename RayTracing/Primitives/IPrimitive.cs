﻿using System.Windows.Media;
using RayTracing.Models;

namespace RayTracing.Primitives {
	interface IPrimitive {
		Color Color { get; }
		int Specular { get; }
		float Reflect { get; }
	}
}