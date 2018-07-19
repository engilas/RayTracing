﻿using System;
using OpenTK;

namespace DrawOpenGL.Models
{
	class Vector {
		public readonly float D1, D2, D3;
        
		public Vector((float, float, float) v) {
			(D1, D2, D3) = v;
		}

		public Vector(float v1, float v2, float v3) {
			D1 = v1;
			D2 = v2;
			D3 = v3;
		}

		public Vector Add(Vector v) {
			return new Vector(D1 + v.D1, D2 + v.D2, D3 + v.D3);
		}

		public Vector Multiply(float k) {
			return new Vector(k * D1, k * D2, k * D3);
		}

		public float Lenght() {
			return (float) Math.Sqrt(DotProduct(this));
		}

		public Vector Subtract(Vector v) {
			return new Vector(D1 - v.D1, D2 - v.D2, D3 - v.D3);
		}

		public float DotProduct(Vector v) {
			return D1 * v.D1 + D2 * v.D2 + D3 * v.D3;
		}

        public Color ToColor()
        {
            return new Color((int)D1, (int)D2, (int)D3, 255);
        }

		/// <summary>
		/// Clamps a color to the canonical color range.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public Vector Clamp() {
			return new Vector(Math.Min(255, (int) Math.Max(0d, D1)),
				(int) Math.Min(255, Math.Max(0d, D2)),
				(int) Math.Min(255, Math.Max(0d, D3)));
		}

		public static Vector FromColor(Color c) {
			return new Vector(c.R, c.G, c.B);
		}
	}
}
