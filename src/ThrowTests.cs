using System;
using System.Drawing;
internal static class ThrowTests
{
 static void Check(bool value,string name) {if(!value)throw new Exception(name);}
 static void Main()
 {
  FlickGesture drag=new FlickGesture();drag.Begin(0,0);drag.Move(.08,400);Check(drag.Release(.10,500)>3500,"fast right flick");
  drag.Begin(0,500);drag.Move(.08,100);Check(drag.Release(.1,0)<-3500,"fast left flick");
  drag.Begin(0,0);drag.Move(.1,10);Check(drag.Release(.2,20)==0,"slow drag is placement");
  drag.Begin(0,0);drag.Move(.05,400);Check(drag.Release(.5,400)==0,"held after drag is not thrown");
  Rectangle area=new Rectangle(-1920,0,1920,1040);ThrowMotion motion=new ThrowMotion();motion.Launch(-1000,2000);
  double initial=motion.X;motion.Step(.016,area,176);Check(motion.X>initial&&motion.Active,"right glide");
  motion.Launch(-180,3000);motion.Step(.1,area,176);Check(motion.Velocity<0,"right edge rebounds");
  motion.Launch(-1919,-3000);motion.Step(.1,area,176);Check(motion.Velocity>0,"left edge rebounds");
  for(int i=0;i<3000;i++) {motion.Step(.016,area,176);Check(motion.X>=area.Left&&motion.X<=area.Right-176,"screen bounds");}
  Check(!motion.Active,"friction settles");motion.Launch(-1000,2000);motion.Step(.1,area,176);double phase=motion.Phase;Check(phase>0,"rotation advances");motion.Stop();Check(!motion.Active,"grab cancels throw");
  motion.Launch(0,4000);motion.Step(.1,new Rectangle(0,0,100,100),176);Check(!motion.Active&&motion.X==0,"tiny screen safe");
  drag.Begin(0,0,500);drag.Move(.08,0,100);PointF v=drag.Release(.1,0,0);Check(v.Y<-3500&&v.X==0,"vertical flick");
  drag.Begin(0,0,0);drag.Move(.08,300,300);v=drag.Release(.1,500,500);Check(Math.Abs(Math.Sqrt(v.X*v.X+v.Y*v.Y)-4200)<1,"diagonal vector limit");
  Rectangle plane=new Rectangle(-1920,-1080,1920,1080);motion.Launch(-1000,-600,800,-1400);double y0=motion.Y;
  motion.Step(.05,plane,176,160);Check(motion.Y<y0&&motion.X>-1000,"diagonal XY motion");
  for(int i=0;i<4000;i++) {
   motion.Step(.008,plane,176,160);
   Check(motion.Y>=plane.Top&&motion.Y<=plane.Bottom-160,"Y bounds");
   Check(motion.X>=plane.Left&&motion.X<=plane.Right-176,"X bounds");
  }
  Check(!motion.Active,"XY friction settles");
  motion.Launch(-1000,-1079,0,-3000);motion.Step(.1,plane,176,160);Check(motion.VelocityY>0,"top rebounds");
  motion.Launch(-1000,-161,0,3000);motion.Step(.1,plane,176,160);Check(motion.VelocityY<0,"bottom rebounds");
  Console.WriteLine("PASS: XY vertical/diagonal flick, radial speed limit, top/bottom rebounds, negative-origin bounds, eventual rest; no Z");
  Console.WriteLine("PASS: flick left/right, slow/held placement, inertia, rebounds, negative bounds, friction settles, animation phase, catch and tiny screen");
 }
}


