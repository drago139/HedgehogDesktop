using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

internal static class AssetTests
{
    [DllImport("user32.dll")] static extern bool DestroyIcon(IntPtr handle);
    static int Main(string[] args)
    {
        string[] files=Directory.GetFiles(args[0],"frame-*.png");Array.Sort(files,StringComparer.Ordinal);
        if(files.Length!=75) throw new Exception("Frame count changed");
        HashSet<string> hashes=new HashSet<string>();
        foreach(string file in files)
        {
            using(SHA256 hash=SHA256.Create()) hashes.Add(Convert.ToBase64String(hash.ComputeHash(File.ReadAllBytes(file))));
            using(Bitmap bitmap=new Bitmap(file))
            {
                if(bitmap.Width!=220||bitmap.Height!=200)throw new Exception("Wrong frame dimensions");
                if(bitmap.GetPixel(0,0).A!=0||bitmap.GetPixel(219,0).A!=0||bitmap.GetPixel(0,199).A!=0||bitmap.GetPixel(219,199).A!=0)throw new Exception("Background is not transparent");
                int opaque=0;
                for(int y=0;y<200;y+=2)for(int x=0;x<220;x+=2)if(bitmap.GetPixel(x,y).A>200)opaque++;
                if(opaque<3000)throw new Exception("Foreground unexpectedly lost");
            }
        }
        if(hashes.Count<15)throw new Exception("Animation does not have enough distinct frames");
        using(Bitmap first=new Bitmap(files[0]))
        {
            Color nose=first.GetPixel(38,120);
            if(nose.A!=255||nose.R>60)throw new Exception("Dark nose was lost");

        }
        Console.WriteLine("PASS: 75 RGBA frames, transparent corners, retained silhouette and dark nose, "+hashes.Count+" distinct animation frames");
        return 0;
    }
}
