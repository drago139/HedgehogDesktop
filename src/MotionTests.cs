using System;
using System.Drawing;

internal static class MotionTests
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static int Main()
    {
        Rectangle bounds = new Rectangle(-1280, 30, 1280, 994);
        Walker walker = new Walker(7);
        walker.Place(-900, 300, bounds, 176, 160);
        double x = walker.X, y = walker.Y;
        walker.Paused = true;
        walker.Step(1.0, bounds, 176, 160);
        Check(walker.X == x && walker.Y == y, "Pause must preserve position");
        walker.Paused = false;
        bool moved = false;
        for (int i=0;i<10000;i++)
        {
            walker.Step(0.04, bounds, 176, 160);
            Check(walker.X >= bounds.Left && walker.X <= bounds.Right-176, "X outside work area");
            Check(walker.Y >= bounds.Top && walker.Y <= bounds.Bottom-160, "Y outside work area");
            moved |= Math.Abs(walker.X-x) + Math.Abs(walker.Y-y)>1;
        }
        Check(moved,"Walker must actually travel");
        walker.BottomOnly = true;
        walker.Place(-300, -2000, bounds, 176, 160);
        for(int i=0;i<2000;i++) walker.Step(0.04,bounds,176,160);
        Check(Math.Abs(walker.Y-(bounds.Bottom-160))<1,"Bottom mode must stay on bottom edge");
        walker.Place(20000,20000,new Rectangle(0,0,100,80),176,160);
        walker.Step(0.04,new Rectangle(0,0,100,80),176,160);
        Check(walker.X==0&&walker.Y==0,"Small monitor must clamp safely");
        Console.WriteLine("PASS: pause, movement, 10,000 bounded steps, negative monitor origin, bottom mode, tiny work area");
        return 0;
    }
}
