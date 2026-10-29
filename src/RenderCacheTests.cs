using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

internal static class RenderCacheTests
{
    [DllImport("gdi32.dll")] static extern IntPtr GetCurrentObject(IntPtr dc,int kind);
    [DllImport("gdi32.dll")] static extern int GetBitmapBits(IntPtr bitmap,int count,byte[] bytes);
    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr bitmap);
    [DllImport("user32.dll")] static extern int GetGuiResources(IntPtr process,int flag);
    static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
    static void Main()
    {
        int before=GetGuiResources(Process.GetCurrentProcess().Handle,0),compared=0;
        try
        {
            foreach(int width in new int[]{132,176,242})
            for(int state=0;state<3;state++)
            for(int frame=0;frame<(state==2?78:75);frame++)
            {
                int height=width*200/220;bool thrown=state==2,right=state==1;
                Native.Surface cached=PetRenderCache.Get(width,frame,thrown,right);
                Check(Object.ReferenceEquals(cached,PetRenderCache.Get(width,frame,thrown,right)),"frame reused");
                using(Bitmap expected=new Bitmap(width,height,PixelFormat.Format32bppPArgb))
                {
                    using(Graphics g=Graphics.FromImage(expected))
                    {
                        g.Clear(Color.Transparent);g.CompositingMode=CompositingMode.SourceCopy;
                        g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
                        if(right){g.TranslateTransform(width,0);g.ScaleTransform(-1,1);}
                        g.DrawImage(thrown?PetFrames.ThrowFrames[frame]:PetFrames.Frames[frame],new Rectangle(0,0,width,height),0,0,220,200,GraphicsUnit.Pixel);
                    }
                    PetHitArea.MakeClickable(expected);
                    IntPtr bitmap=expected.GetHbitmap(Color.FromArgb(0));
                    try
                    {
                        byte[] a=new byte[width*height*4],b=new byte[a.Length];
                        Check(GetBitmapBits(bitmap,a.Length,a)==a.Length,"reference read");
                        Check(GetBitmapBits(GetCurrentObject(cached.DC,7),b.Length,b)==b.Length,"cache read");
                        for(int i=0;i<a.Length;i++)if(a[i]!=b[i])throw new Exception("pixel mismatch "+width+"/"+state+"/"+frame+" byte"+i);
                    }
                    finally{DeleteObject(bitmap);}
                }
                Check(PetRenderCache.Bytes<=64L*1024*1024&&PetRenderCache.Count<=512,"bounded cache");compared++;
            }
            Check(PetRenderCache.Hits>=684,"shared cache hits");
            PetRenderCache.Release();Check(PetRenderCache.Count==0&&PetRenderCache.Bytes==0,"release accounting");
            PetFrames.Release();GC.Collect();GC.WaitForPendingFinalizers();
            Check(GetGuiResources(Process.GetCurrentProcess().Handle,0)<=before+8,"GDI resources released");
            Console.WriteLine("PASS: "+compared+" cached frames byte-identical to old renderer at three sizes, both facings, all throw frames; reuse, LRU budget and GDI cleanup");
        }
        finally{PetRenderCache.Release();PetFrames.Release();}
    }
}
