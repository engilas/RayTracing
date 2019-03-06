namespace RayTracing.Models
{
    internal class Pixel
    {
        public Pixel(double x, double y, Vector c)
        {
            X = x;
            Y = y;
            Color = c;
        }

        public double X { get; }
        public double Y { get; }
        public Vector Color { get; }
    }
}