using System;
using System.Drawing;
using System.Runtime.InteropServices;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)] internal struct Point { public int X,Y; public Point(int x,int y) {X=x;Y=y;} }
    [StructLayout(LayoutKind.Sequential)] internal struct Size { public int W,H; public Size(int w,int h) {W=w;H=h;} }
    [StructLayout(LayoutKind.Sequential,Pack=1)] internal struct Blend { public byte Op,Flags,Alpha,Format; }
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr window);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr window,IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] static extern IntPtr SelectObject(IntPtr dc,IntPtr item);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr item);
    [DllImport("user32.dll",SetLastError=true)] static extern bool UpdateLayeredWindow(IntPtr window,IntPtr destination,ref Point position,ref Size size,IntPtr source,ref Point origin,int key,ref Blend blend,int flags);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr window,IntPtr after,int x,int y,int w,int h,uint flags);
    [DllImport("user32.dll")] internal static extern bool DestroyIcon(IntPtr icon);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern IntPtr FindWindow(string className,string name);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")] internal static extern bool PostMessage(IntPtr window,uint message,IntPtr w,IntPtr l);

    internal static void Display(IntPtr window,Bitmap bitmap,int x,int y)
    {
        IntPtr screen=GetDC(IntPtr.Zero),memory=CreateCompatibleDC(screen),image=IntPtr.Zero,old=IntPtr.Zero;
        try
        {
            image=bitmap.GetHbitmap(Color.FromArgb(0)); old=SelectObject(memory,image);
            Point position=new Point(x,y),origin=new Point(0,0);Size size=new Size(bitmap.Width,bitmap.Height);
            Blend blend=new Blend { Op=0,Flags=0,Alpha=255,Format=1 };
            if (!UpdateLayeredWindow(window,screen,ref position,ref size,memory,ref origin,0,ref blend,2))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            if(old!=IntPtr.Zero) SelectObject(memory,old);
            if(image!=IntPtr.Zero) DeleteObject(image);
            if(memory!=IntPtr.Zero) DeleteDC(memory);
            if(screen!=IntPtr.Zero) ReleaseDC(IntPtr.Zero,screen);
        }
    }

    // A prepared frame owns its GDI resources once, rather than per pet per tick.
    internal sealed class Surface : IDisposable
    {
        internal IntPtr DC;
        IntPtr image,old;
        internal readonly int Width,Height;
        internal Surface(Bitmap bitmap)
        {
            Width=bitmap.Width;Height=bitmap.Height;
            try
            {
                DC=CreateCompatibleDC(IntPtr.Zero);
                if(DC==IntPtr.Zero)throw new System.ComponentModel.Win32Exception();
                image=bitmap.GetHbitmap(Color.FromArgb(0));old=SelectObject(DC,image);
                if(old==IntPtr.Zero||old==new IntPtr(-1))throw new System.ComponentModel.Win32Exception();
            }
            catch {Dispose();throw;}
        }
        public void Dispose()
        {
            if(DC!=IntPtr.Zero&&old!=IntPtr.Zero&&old!=new IntPtr(-1))SelectObject(DC,old);
            if(image!=IntPtr.Zero)DeleteObject(image);
            if(DC!=IntPtr.Zero)DeleteDC(DC);
            DC=image=old=IntPtr.Zero;
        }
    }
    internal static void Display(IntPtr window,Surface surface,int x,int y)
    {
        Point position=new Point(x,y),origin=new Point(0,0);Size size=new Size(surface.Width,surface.Height);
        Blend blend=new Blend {Op=0,Flags=0,Alpha=255,Format=1};
        if(!UpdateLayeredWindow(window,IntPtr.Zero,ref position,ref size,surface.DC,ref origin,0,ref blend,2))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
}
