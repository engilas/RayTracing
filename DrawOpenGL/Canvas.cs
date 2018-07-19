using System;
using System.Collections.Concurrent;
using DrawOpenGL.Models;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DrawOpenGL {
	class Canvas : GameWindow, ICanvas {
		
		private ConcurrentBag<Pixel> _points = new ConcurrentBag<Pixel>();
		private volatile bool _stop = false;

		private readonly Color _bgColor;

		public Canvas(int width, int height, Color bgColor) : base(width, height) {
			_bgColor = bgColor;
		}

		public void DrawPoint(int x, int y, Color color) {
            if (Math.Abs(x) > Width * 2 || Math.Abs(y) > Height * 2)
                throw new ArgumentOutOfRangeException();

            var xScale = x / (float) Width * 2;
			var yScale = y / (float) Height * 2;


			_points.Add(new Pixel(xScale, yScale, color));
		}

		public void Clear() {
			_points.Clear();
		}

		public void Stop() {
			Close();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			GL.ClearColor(_bgColor);
		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			if (_stop) {
				Dispose();
				return;
			}

			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();

			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();

			var items = _points.ToArray();

			GL.Begin(PrimitiveType.Points);
			for (int i = 0; i < items.Length; i++) {
				var item = items[i];
				GL.Color3(item.Color);
				GL.Vertex2(item.X, item.Y);
			}
			GL.End();
			
			GL.Flush();
			SwapBuffers();
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);

			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

			Matrix4 projection =
				Matrix4.CreatePerspectiveFieldOfView((float) Math.PI / 4, Width / (float) Height, 1.0f, 64.0f);

			GL.MatrixMode(MatrixMode.Projection);

			GL.LoadMatrix(ref projection);
		}
	}
}
