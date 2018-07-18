using System;
using System.Collections.Concurrent;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace DrawOpenGL {
	class Canvas : GameWindow, ICanvas {
		struct Pixel {
			public float X { get; }
			public float Y { get; }
			public Color Color { get; }

			public Pixel(float x, float y, Color c) {
				X = x;
				Y = y;
				Color = c;
			}
		}
		
		private ConcurrentBag<Pixel> _points = new ConcurrentBag<Pixel>();
		private volatile bool _stop = false;

		public Canvas(int width, int height) : base(width, height) {
		}

		public void DrawPoint(int x, int y, Color color) {
            if (Math.Abs(x) > Width || Math.Abs(y) > Height)
                throw new ArgumentOutOfRangeException();

            var xScale = x / (float) Width;
			var yScale = y / (float) Height;


			_points.Add(new Pixel(xScale, yScale, color));
		}

		public void Stop() {
			Close();
		}

		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);

			GL.ClearColor(Color.Black);
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
			GL.Color3(1, 1, 1);
			//GL.PointSize(5);

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
