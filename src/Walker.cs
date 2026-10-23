using System;
using System.Drawing;

internal sealed class Walker
{
    readonly Random random;
    double targetX, targetY, wait;
    bool hasTarget;
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Speed = 55;
    public bool Paused;
    public bool BottomOnly;
    public bool FacingRight { get; private set; }
    public bool Walking { get; private set; }
    public double DirectionX {get;private set;}
    public double DirectionY {get;private set;}

    public Walker(int seed) { random = new Random(seed);DirectionX=-1; }
    static double Clamp(double v, double a, double b) { return Math.Max(a, Math.Min(Math.Max(a,b),v)); }

    public void Place(double x, double y, Rectangle area, int width, int height)
    {
        X = Clamp(x,area.Left,area.Right-width);
        Y = Clamp(y,area.Top,area.Bottom-height);
        if (BottomOnly) Y = Math.Max(area.Top,area.Bottom-height);
        hasTarget = false; wait = 0; Walking = false;
    }

    public void Step(double seconds, Rectangle area, int width, int height)
    {
        X = Clamp(X,area.Left,area.Right-width);
        Y = Clamp(Y,area.Top,area.Bottom-height);
        if (BottomOnly) Y = Math.Max(area.Top,area.Bottom-height);
        Walking = false;
        if (Paused) return;
        double dt = Clamp(seconds,0,0.1);
        if (wait>0) { wait -= dt; return; }
        if (!hasTarget)
        {
            double rangeX = Math.Max(0,area.Width-width);
            double rangeY = Math.Max(0,area.Height-height);
            targetX = area.Left + random.NextDouble()*rangeX;
            targetY = BottomOnly ? Math.Max(area.Top,area.Bottom-height) : area.Top+random.NextDouble()*rangeY;
            hasTarget = true;
        }
        targetX = Clamp(targetX,area.Left,area.Right-width);
        targetY = BottomOnly ? Math.Max(area.Top,area.Bottom-height) : Clamp(targetY,area.Top,area.Bottom-height);
        double dx = targetX-X, dy = targetY-Y;
        double distance = Math.Sqrt(dx*dx+dy*dy);
        if(distance>0.01) {DirectionX=dx/distance;DirectionY=dy/distance;}
        if (Math.Abs(dx)>0.25) FacingRight = dx>0;
        double amount = Math.Max(1,Speed)*dt;
        if (distance<=Math.Max(0.5,amount))
        {
            X=targetX; Y=targetY; hasTarget=false; wait=0.5+random.NextDouble()*2.5;
            return;
        }
        X += dx/distance*amount;
        Y += dy/distance*amount;
        Walking = amount>0;
    }
}
