using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RayTracing
{
    class Canvas : ICanvas
    {
        private readonly int _width;
        private readonly int _height;
        private WriteableBitmap _image;

        public Canvas(int width, int height)
        {
            _width = width;
            _height = height;
            InitializeImage();
        }

	    public void DrawPoint(int x, int y, Color color) {
		    var xScale = _width / 2f + x;
		    var yScale = _height / 2f - y;

		    var colorData = new byte [] {color.B, color.G, color.R, 0};

		    var rect = new Int32Rect((int) Math.Round(xScale), (int) Math.Round(yScale), 1, 1);
		    _image.WritePixels(rect, colorData, 4, 0);
	    }

	    public void ClearImage()
        {
            InitializeImage();
        }

        public BitmapSource GetImage()
        {
            return _image;
        }

        public byte[] GetBytes()
        {
            int size = _width * _height * _image.Format.BitsPerPixel / 8;
            byte[] arr = new byte[size];
            IntPtr buffer = _image.BackBuffer;
            Marshal.Copy(buffer, arr, 0, size);
            return arr;
        }

        private void InitializeImage()
        {
            _image = new WriteableBitmap(_width,
                _height, 96, 96, PixelFormats.Bgr32, null);
            var pixels = Enumerable.Repeat<byte>(0xff, _width * _height * _image.Format.BitsPerPixel / 8).ToArray();
            var rect = new Int32Rect(0, 0, _width, _height);
            _image.WritePixels(rect, pixels, _image.PixelWidth * _image.Format.BitsPerPixel / 8, 0);
        }
    }
}
