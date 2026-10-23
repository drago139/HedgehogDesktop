using System;
using System.Collections.Generic;
using System.Drawing;
internal sealed class FlickGesture
{
    struct Sample {internal double Time,X,Y;internal Sample(double t,double x,double y) {Time=t;X=x;Y=y;}}
    readonly List<Sample> samples=new List<Sample>();
    internal void Begin(double time,double x) {Begin(time,x,0);}
    internal void Begin(double time,double x,double y) {samples.Clear();samples.Add(new Sample(time,x,y));}
    internal void Move(double time,double x) {Move(time,x,0);}
    internal void Move(double time,double x,double y)
    {
        samples.Add(new Sample(time,x,y));
        while(samples.Count>2&&samples[1].Time<time-0.12)samples.RemoveAt(0);
    }
    internal double Release(double time,double x) {return Release(time,x,0).X;}
    internal PointF Release(double time,double x,double y)
    {
        Move(time,x,y);Sample first=samples[0];double elapsed=time-first.Time;
        if(elapsed<0.015||elapsed>0.25)return PointF.Empty;
        double vx=(x-first.X)/elapsed,vy=(y-first.Y)/elapsed,speed=Math.Sqrt(vx*vx+vy*vy);
        if(speed<350)return PointF.Empty;
        double factor=Math.Min(1,4200/speed);return new PointF((float)(vx*factor),(float)(vy*factor));
    }

}
internal sealed class ThrowMotion
{
    internal double X {get;private set;}
    internal double Y {get;private set;}
    internal double Velocity {get {return VelocityX;}}
    internal double VelocityX {get;private set;}
    internal double VelocityY {get;private set;}
    internal double Phase {get;private set;}
    internal double Speed {get {return Math.Sqrt(VelocityX*VelocityX+VelocityY*VelocityY);}}
    internal bool Active {get {return Speed>=45;}}
    internal void Launch(double x,double velocity) {Launch(x,0,velocity,0);}
    internal void Launch(double x,double y,double vx,double vy)
    {
        X=x;Y=y;Phase=0;
        double speed=Math.Sqrt(vx*vx+vy*vy),factor=speed>4200?4200/speed:1;
        VelocityX=vx*factor;VelocityY=vy*factor;
    }
    internal void Stop() {VelocityX=VelocityY=0;}
    internal void Step(double seconds,Rectangle area,int width) {Step(seconds,area,width,0);}
    internal void Step(double seconds,Rectangle area,int width,int height)
    {
        if(!Active||seconds<=0||Double.IsNaN(seconds)||Double.IsInfinity(seconds))return;
        double remaining=Math.Min(seconds,0.25),left=area.Left,right=Math.Max(left,area.Right-width);
        double top=area.Top,bottom=Math.Max(top,area.Bottom-height);
        if(right<=left&&bottom<=top) {X=left;Y=top;Stop();return;}
        while(remaining>0&&Active)
        {
            double dt=Math.Min(0.008,remaining);remaining-=dt;
            X+=VelocityX*dt;Y+=VelocityY*dt;
            if(right<=left) {X=left;VelocityX=0;}
            else {
                if(X<left) {X=left+(left-X);VelocityX=Math.Abs(VelocityX)*0.82;}
                if(X>right) {X=right-(X-right);VelocityX=-Math.Abs(VelocityX)*0.82;}
            }
            if(bottom<=top) {Y=top;VelocityY=0;}
            else {
                if(Y<top) {Y=top+(top-Y);VelocityY=Math.Abs(VelocityY)*0.82;}
                if(Y>bottom) {Y=bottom-(Y-bottom);VelocityY=-Math.Abs(VelocityY)*0.82;}
            }
            X=Math.Max(left,Math.Min(right,X));Y=Math.Max(top,Math.Min(bottom,Y));
            Phase+=dt*(5+19*Math.Min(1,Speed/1800));
            double friction=Math.Exp(-0.65*dt);VelocityX*=friction;VelocityY*=friction;
        }
        if(!Active)Stop();
    }
}
