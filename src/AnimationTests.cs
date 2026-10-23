using System;

internal static class AnimationTests
{
    static void Check(bool value,string message) {if(!value)throw new Exception(message);}
    static int Main()
    {
        FrameClock clock=new FrameClock(75,25);
        for(int i=0;i<25;i++)clock.Advance(0.04,false);
        Check(clock.Frame==25,"One second must advance 25 frames, including idle/hover time");
        int before=clock.Frame;
        clock.Advance(1.0,true);
        Check(clock.Frame==before,"Explicit pause must freeze animation");
        clock.Advance(0.8,false);
        Check(clock.Frame==45,"A late timer must catch up to real elapsed time");
        clock.Advance(1.2,false);
        Check(clock.Frame==0,"The three-second clip must loop without slowing");
        clock.Advance(-1,false);
        Check(clock.Frame==0,"Negative time must not rewind animation");
        Console.WriteLine("PASS: constant25fps, idle/hover timing, explicit pause, late-timer catch-up, 3s loop, negative elapsed");
        return 0;
    }
}
