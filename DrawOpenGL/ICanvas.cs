using DrawOpenGL.Models;
using OpenTK;

namespace DrawOpenGL
{
    interface ICanvas {
	    void DrawPoint(int x, int y, Color color);
	    void Clear();

		int Width { get; }
	    int Height { get; }
    }
}
