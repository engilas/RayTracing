using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media;
using RayTracing.Annotations;
using RayTracing.Models;
using RayTracing.Primitives;

namespace RayTracing {
	public class MainViewModel : INotifyPropertyChanged {
		private readonly Canvas _canvas;
		private int _width;
		private int _height;

		public ImageSource Image => _canvas.GetImage();

		public int Width {
			get => _width;
			set {
				if (value == _width) return;
				_width = value;
				OnPropertyChanged();
			}
		}

		public int Height {
			get => _height;
			set {
				if (value == _height) return;
				_height = value;
				OnPropertyChanged();
			}
		}

		public MainViewModel() {

			Width = 300;
			Height = 300;
			var bg = Color.FromRgb(0, 0, 0);

			var scene = new Scene {
				Spheres = new List<Sphere>(new[] {
					new Sphere {
						Center = new Vector(0, -1, 3),
						Radius = 1,
						Color = Color.FromRgb(255, 0, 0),
						Specular = 500,
						Reflect = 0.2f
					},
					new Sphere {
						Center = new Vector(-2, 0, 4),
						Radius = 1,
						Color = Color.FromRgb(0, 0, 255),
						Specular = 500,
						Reflect = 0.3f
					},
					new Sphere {
						Center = new Vector(2, 0, 4),
						Radius = 1,
						Color = Color.FromRgb(0, 255, 0),
						Specular = 10,
						Reflect = 0.4f
					},
					new Sphere {
						Center = new Vector(0, -5001, 0),
						Radius = 5000f,
						Color = Color.FromRgb(255, 255, 0),
						Specular = 1000,
						Reflect = 0.5f
					}
				}),
				Lights = new List<Light>(
					new[] {
						new Light {
							Type = LightType.Ambient,
							Intensity = 0.2f
						},
						new Light {
							Type = LightType.Point,
							Intensity = 0.6f,
							Position = new Vector(2, 1, 0)
						},
						new Light {
							Type = LightType.Direct,
							Intensity = 0.2f,
							Direction = new Vector(1, 4, 4)
						}
					}
				)
			};

			var options = new RenderOptions {
				BgColor = bg,
				CameraPos = new Vector(0, 0, 0),
				ViewportWidth = 1,
				ViewportHeight = 1,
				CanvasWidth = Width,
				CanvasHeight = Height,
				ViewportDistance = 1,
				RecursionDepth = 3,
                //CameraRotation = new[,] {
                //    {0.7071f, 0, -0.7071f},
                //    {0, 1, 0},
                //    {0.7071f, 0, 0.7071f}
                //}
            };

			Console.WriteLine("Started");

			_canvas = new Canvas(Width, Height);
			
			var render = new Render(_canvas, scene, options);

			ProcessRender(render);
		}

		void ProcessRender(Render render) {
			
			var sw = Stopwatch.StartNew();

			render.Process();
			
			sw.Stop();

			Console.WriteLine($@"Rendered in {sw.Elapsed} ms");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}