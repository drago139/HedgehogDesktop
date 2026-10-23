using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
internal static class PetFrames
{
    static List<Bitmap> frames,throwFrames;
    internal static List<Bitmap> ThrowFrames {get {if(throwFrames==null)throwFrames=LoadThrow();return throwFrames;}}
    static List<Bitmap> LoadThrow()
    {
        List<Bitmap> loaded=new List<Bitmap>();
        try {
            using(Stream resource=Assembly.GetExecutingAssembly().GetManifestResourceStream("ThrowFrames"))
            using(ZipArchive zip=new ZipArchive(resource,ZipArchiveMode.Read))
            {
                List<ZipArchiveEntry> entries=new List<ZipArchiveEntry>(zip.Entries);
                entries.Sort(delegate(ZipArchiveEntry a,ZipArchiveEntry b) {return String.CompareOrdinal(a.FullName,b.FullName);});
                foreach(ZipArchiveEntry entry in entries)using(Stream stream=entry.Open())using(Bitmap bitmap=new Bitmap(stream))loaded.Add(new Bitmap(bitmap));
            }
            if(loaded.Count!=78)throw new InvalidDataException("Expected78 throw frames");return loaded;
        } catch {foreach(Bitmap frame in loaded)frame.Dispose();throw;}
    }
    internal static List<Bitmap> Frames
    {
        get
        {
            if(frames!=null)return frames;
            List<Bitmap> loaded=new List<Bitmap>();
            try
            {
                using(Stream resource=Assembly.GetExecutingAssembly().GetManifestResourceStream("HedgehogFrames"))
                using(ZipArchive zip=new ZipArchive(resource,ZipArchiveMode.Read))
                {
                    List<ZipArchiveEntry> entries=new List<ZipArchiveEntry>(zip.Entries);
                    entries.Sort(delegate(ZipArchiveEntry a,ZipArchiveEntry b) {return String.CompareOrdinal(a.FullName,b.FullName);});
                    foreach(ZipArchiveEntry entry in entries)
                    {
                        if(!entry.Name.EndsWith(".png",StringComparison.OrdinalIgnoreCase))continue;
                        using(Stream stream=entry.Open())using(Bitmap bitmap=new Bitmap(stream))loaded.Add(new Bitmap(bitmap));
                    }
                }
                if(loaded.Count!=75)throw new InvalidDataException("Expected75 animation frames");
                frames=loaded;return frames;
            }
            catch {foreach(Bitmap frame in loaded)frame.Dispose();throw;}
        }
    }
    internal static void Release() {if(frames!=null)foreach(Bitmap frame in frames)frame.Dispose();if(throwFrames!=null)foreach(Bitmap frame in throwFrames)frame.Dispose();frames=null;throwFrames=null;}
}
