using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using RayTracing.Primitives;
using RayTracing.Properties;
using Vector = RayTracing.Models.Vector;

namespace RayTracing
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Canvas _canvas;
        private int _height;
        private int _width;

        public MainViewModel()
        {
            Width = 800;
            Height = 800;
            var bg = Color.FromRgb(15, 211, 255);

            var pointLight = new Vector(3, 5, 0);

            var scene = new Scene
            {
                Spheres = new List<Sphere>
                {
//                    new Sphere
//                    {
//                        Center = new Vector(0, -1, 5),
//                        Radius = 1,
//                        Color = Color.FromRgb(255, 0, 0),
//                        Specular = 500,
//                        Reflect = 0.2
//                    },
                    new Sphere
                    {
                        Center = new Vector(2, 0, 6),
                        Radius = 1,
                        Color = Color.FromRgb(0, 0, 255),
                        Specular = 500,
                        Reflect = 0.3
                    },
                    //new Sphere {
                    //    Center = new Vector(-2, 0, 6),
                    //    Radius = 1,
                    //    Color = Color.FromRgb(0, 255, 0),
                    //    Specular = 10,
                    //    Reflect = 0.4
                    //},
                    //new Sphere {
                    // Center = new Vector(-5, 5, 6),
                    // Radius = 1,
                    // Color = Color.FromRgb(0, 0, 255),
                    // Specular = 500,
                    // Reflect = 0.5
                    //},
                    new Sphere
                    {
                        Color = Color.FromRgb(255, 255, 255),
                        Radius = 0.1,
                        Center = pointLight,
                        Reflect = 0,
                        Specular = -1,
                        LightTransparent = true
                    }
                },
                Lights = new List<Light>
                {
                    new Light
                    {
                        Type = LightType.Ambient,
                        Intensity = 0.2
                    },
                    new Light
                    {
                        Type = LightType.Point,
                        Intensity = 0.6,
                        Position = pointLight
                    },
                    new Light
                    {
                        Type = LightType.Direct,
                        Intensity = 0.2,
                        Direction = new Vector(3, -1, 1)
                    }
                },
                Planes = new List<Plane>
                {
                    new Plane(
                        0,
                        -1,
                        0,
                        -1
                    )
                    {
                        Color = Color.FromRgb(255, 255, 0),
                        Reflect = 0.3,
                        Specular = 100
                    },
                    new Plane(
                        0,
                        0,
                        -1,
                        15
                    )
                    {
                        Color = Color.FromRgb(110, 157, 153),
                        Reflect = 0.3,
                        Specular = 50
                    }
                },
                Boxes = new List<Box>
                {
                    new Box
                    {
                        Color = Color.FromRgb(255, 147, 0),
                        Min = new Vector(-1.5, 0.3, 7),
                        Max = new Vector(-0.5, 2.5, 8),
                        Reflect = 0.2,
                        Specular = 200
                    }
                },
                Toruses =
                {
                    new Torus(0.4, 1)
                    {
                        Position = new Vector(-2, 0, 6),
                        Rotation = new RotationMatrix(0, 0, 90),
                        Color = Colors.GreenYellow,
                        Reflect = 0.7
                    }
                },
                Surfaces = new List<Surface>
                {
//                    new Surface(Surface.GetEllipsoid(5, 5, 5))
//                    {
//                        Color = Color.FromRgb(255, 0, 235),
//                        Reflect = 0.4,
//                        Specular = 200,
//
//                        Position = new Vector(0, 0, 5),
//                        //Rotation = new RotationMatrix(-90, 0, 0)
//                        ZMin = -2,
//                        XMin = -1,
//                        XMax = 1
//                    },
                    new Surface(Surface.GetEllipticCone(1/3d, 1/3d, 1d))
                    {
                        Color = Color.FromRgb(234, 17, 82),
                        Reflect = 0.2,
                        Specular = 200,
                        Position = new Vector(-5, 4, 6),
                        Rotation = new RotationMatrix(90, 0, 0),
                        YMax = 4
                    }, new Surface(Surface.GetEllipticCylinder(1/2d, 1/2d))
                    {
                        Color = Color.FromRgb(204, 255, 0),
                        Reflect = 0.2,
                        Specular = 200,
                        Position = new Vector(0, 0, 6),
                        Rotation = new RotationMatrix(90, 0, 0),
                        YMin = -1,
                        YMax = 1
                        
                    }
                },
//                Disks = new List<Disk>
//                {
//                    new Disk(1, 1, 1)
//                    {
//                        Color = Colors.Chocolate,
//                        Reflect = 0.1,
//                        Specular = 100,
//                        Position = new Vector(2, 0, 0),
//                        Rotation = new RotationMatrix(-35, 0, 0)
//                    }
//                }
            };

            var options = new RenderOptions
            {
                BgColor = bg,
                //CameraPos = new Vector(-2, 0, -2),
                //CameraPos = new Vector(1, 5, 0),
                //CameraPos = new Vector(5, 0, 0),
                //CameraPos = new Vector(1.75, 0.5, 4),
                //CameraPos = new Vector(0, 0, -2.4),
                //CameraPos = new Vector(-10, 0, 6),
                //CameraPos = new Vector(0, 0, -10),
                CameraPos = new Vector(3.5, 1.5, -2),
                ViewportWidth = 1,
                ViewportHeight = 1,
                CanvasWidth = Width,
                CanvasHeight = Height,
                ViewportDistance = 1,
                RecursionDepth = 2,
                //CameraRotationZ = -45,
                CameraRotationX = -5,
                CameraRotationY = 30
            };

            Console.WriteLine("Started");

            _canvas = new Canvas(Width, Height);

            var render = new Render(_canvas, scene, options);

            ProcessRender(render);

            //Width = 0;Height=0;
        }

        [UsedImplicitly] public ImageSource Image => _canvas.GetImage();

        public int Width
        {
            get => _width;
            set
            {
                if (value == _width) return;
                _width = value;
                OnPropertyChanged();
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (value == _height) return;
                _height = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ProcessRender(Render render)
        {
            var sw = Stopwatch.StartNew();

            render.Process();

            sw.Stop();

            Console.WriteLine($@"Rendered in {sw.Elapsed} ms");
            MessageBox.Show($@"Rendered in {sw.Elapsed} ms");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}