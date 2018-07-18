using System;
using System.Threading;

namespace DrawOpenGL
{
    public class CanvasManager : IDisposable {
	    private Thread _workerThread;
	    private Canvas _canvas;
	    private readonly ManualResetEventSlim _mres = new ManualResetEventSlim();

	    public ICanvas Canvas => _canvas;
		
	    public void Initialize(double frameRate, int width, int height) {
		    _workerThread = new Thread(() => {
			    _canvas = new Canvas(width, height);
			    _canvas.Load += (_, __) => {_mres.Set(); };
			    _canvas.Run(frameRate);
		    });
		    _workerThread.Start();
		    _mres.Wait();
	    }

	    public void Dispose() {
			_canvas.Stop();
	    }

	    ~CanvasManager() {
			Dispose();
	    }
    }
}
