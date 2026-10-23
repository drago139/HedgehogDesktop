using System;

internal sealed class FrameClock
{
    readonly int count,fps;
    double seconds;
    public FrameClock(int frameCount,int framesPerSecond)
    {
        if(frameCount<=0||framesPerSecond<=0)throw new ArgumentOutOfRangeException();
        count=frameCount;fps=framesPerSecond;
    }
    public int Frame {get {return GetFrame(count,fps);}}
    public int GetFrame(int frameCount,int framesPerSecond)
    {
        if(frameCount<=0||framesPerSecond<=0)throw new ArgumentOutOfRangeException();
        return (int)(Math.Floor(seconds*framesPerSecond+0.0000001)%frameCount);
    }
    public void Advance(double elapsed,bool paused)
    {
        if(paused||elapsed<=0||Double.IsNaN(elapsed)||Double.IsInfinity(elapsed))return;
        seconds+=elapsed;
    }
}
