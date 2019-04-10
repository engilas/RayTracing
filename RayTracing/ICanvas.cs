using System.Windows.Media;

namespace RayTracing
{
    internal interface ICanvas
    {
        void DrawPoint(int x, int y, Color color);
        //void Clear();

        //int Width { get; }
        //   int Height { get; }
        //   void Start();
        //   void Stop();
    }
}