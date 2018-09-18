using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using RayTracing.Models;
using RayTracing.Primitives;
using RayTracing.Properties;

namespace RayTracing {
	public class MainViewModel : INotifyPropertyChanged {
		private readonly Canvas _canvas;
		private int _width;
		private int _height;

		[UsedImplicitly]
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

			Width = 800;
			Height = 800;
			var bg = Color.FromRgb(0, 0, 0);

            var pointLight = new Vector(3, 5, 0);

			var scene = new Scene {
                Spheres = new List<Sphere> {
                    new Sphere {
                        Center = new Vector(0, -1, 5),
                        Radius = 1,
                        Color = Color.FromRgb(255, 0, 0),
                        Specular = 500,
                        Reflect = 0.2
                    },
                    new Sphere {
                        Center = new Vector(2, 0, 6),
                        Radius = 1,
                        Color = Color.FromRgb(0, 0, 255),
                        Specular = 500,
                        Reflect = 0.3
                    },
                    new Sphere {
                        Center = new Vector(-2, 0, 6),
                        Radius = 1,
                        Color = Color.FromRgb(0, 255, 0),
                        Specular = 10,
                        Reflect = 0.4
                    },
                    //new Sphere {
                    //    Center = new Vector(0.7, 1.8, 1.7),
                    //    Radius = 0.6,
                    //    Color = Color.FromRgb(255, 0, 0),
                    //    Specular = 500,
                    //    Reflect = 0.2
                    //},
                    new Sphere
                    {
                        Color = Color.FromRgb(255, 255, 255),
                        Radius = 0.1,
                        Center = pointLight,
                        Reflect = 0,
                        Specular = -1,
                        LightTransparent = true,
                    }
                },
                Lights = new List<Light> {
                    new Light {
                        Type = LightType.Ambient,
                        Intensity = 0.2
                    },
                    new Light {
                        Type = LightType.Point,
                        Intensity = 0.6,
                        Position = pointLight
                    },
                    new Light {
                        Type = LightType.Direct,
                        Intensity = 0.2,
                        Direction = new Vector(3, -1, 1)
                    }
                },
                Planes = new List<Plane> {
                    new Plane {
                        Color = Color.FromRgb(255, 255, 0),
                        A = 0.0,
                        B = 1,
                        C = 0,
                        D = 1,
                        Reflect = 0.3,
                        Specular = 100
                    },
                    new Plane {
                        Color = Color.FromRgb(110, 157, 153),
                        A = 0.0,
                        B = 0,
                        C = -1,
                        D = 15,
                        Reflect = 0.3,
                        Specular = 50
                    }
                },
                Boxes = new List<Box> {
                    new Box {
                        Color = Color.FromRgb(255, 147, 0),
                        Min = new Vector(3.5, 0.3, 6),
                        Max = new Vector(4.5, 3.5, 7),
                        Reflect = 0.2,
                        Specular = 200
                    },
                },
                //Surfaces = new List<Surface>
                //{
                //    new Surface
                //    {
                //        Color = Color.FromRgb(255, 0, 235),
                //        Reflect = 0.2,
                //        Specular = 200,
                //        AxisDirection = Axis.Y,
                //        //Direction = Direction.Down,
                //        Width = 1,
                //        //Edge = 4,
                //        Offset = new Vector(0, 2, 10)
                //    }
                //}
            };

			var options = new RenderOptions {
				BgColor = bg,
                //CameraPos = new Vector(-2, 0, -2),
                //CameraPos = new Vector(1, 5, 0),
                //CameraPos = new Vector(5, 0, 0),
                CameraPos = new Vector(-3, 5, -7),
			    //CameraPos = new Vector(0, 0, -2.4),
                ViewportWidth = 1,
				ViewportHeight = 1,
				CanvasWidth = Width,
				CanvasHeight = Height,
				ViewportDistance = 1,
				RecursionDepth = 3,
				CameraRotationX = -25,
			    CameraRotationY = -25
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