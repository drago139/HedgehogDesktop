using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// UI-thread only. UpdateLayeredWindow copies pixels synchronously, so eviction
// cannot invalidate pixels already displayed by another pet window.
internal static class PetRenderCache
{
    const long Budget=64L*1024*1024;
    const int MaxEntries=512;
    sealed class Entry
    {
        internal int Key;
        internal Native.Surface Surface;
        internal LinkedListNode<Entry> Node;
        internal long Bytes;
    }
    static readonly Dictionary<int,Entry> entries=new Dictionary<int,Entry>();
    static readonly LinkedList<Entry> recent=new LinkedList<Entry>();
    internal static long Hits {get;private set;}
    internal static long Misses {get;private set;}
    internal static long Bytes {get;private set;}
    internal static int Count {get {return entries.Count;}}
    internal static Native.Surface Get(int width,int frame,bool thrown,bool right)
    {
        // Throw frames encode their own rotation; facing is relevant only to walking.
        bool mirrored=!thrown&&right;
        int key=width*1024+(thrown?512:0)+(mirrored?256:0)+frame;
        Entry entry;
        if(entries.TryGetValue(key,out entry))
        {
            Hits++;recent.Remove(entry.Node);recent.AddFirst(entry.Node);return entry.Surface;
        }
        Misses++;int height=width*200/220;long bytes=(long)width*height*4;
        while(entries.Count>0&&(Bytes+bytes>Budget||entries.Count>=MaxEntries))RemoveOldest();
        Native.Surface prepared;
        using(Bitmap output=new Bitmap(width,height,PixelFormat.Format32bppPArgb))
        {
            using(Graphics graphics=Graphics.FromImage(output))
            {
                graphics.Clear(Color.Transparent);graphics.CompositingMode=CompositingMode.SourceCopy;
                graphics.InterpolationMode=InterpolationMode.NearestNeighbor;graphics.PixelOffsetMode=PixelOffsetMode.Half;
                if(mirrored){graphics.TranslateTransform(width,0);graphics.ScaleTransform(-1,1);}
                Bitmap source=thrown?PetFrames.ThrowFrames[frame]:PetFrames.Frames[frame];
                graphics.DrawImage(source,new Rectangle(0,0,width,height),0,0,220,200,GraphicsUnit.Pixel);
            }
            PetHitArea.MakeClickable(output);prepared=new Native.Surface(output);
        }
        entry=new Entry {Key=key,Surface=prepared,Bytes=bytes};entry.Node=recent.AddFirst(entry);
        entries.Add(key,entry);Bytes+=bytes;return prepared;
    }
    static void RemoveOldest()
    {
        Entry entry=recent.Last.Value;recent.RemoveLast();entries.Remove(entry.Key);
        Bytes-=entry.Bytes;entry.Surface.Dispose();
    }
    internal static void Release(){while(entries.Count>0)RemoveOldest();}
}
