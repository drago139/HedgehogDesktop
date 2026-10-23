using System;
using System.Drawing;
using System.Drawing.Imaging;
internal static class HitAreaTests
{
 static void Check(bool ok,string reason) {if(!ok)throw new Exception(reason);}
 static void SourcePoint(string name,int sourceX,int sourceY)
 {
  string[] row=null;foreach(string line in System.IO.File.ReadAllLines("assets/throw/source-mapping.csv"))if(line.StartsWith(name+","))row=line.Split(',');
  Check(row!=null,"source mapping");double left=Double.Parse(row[1],System.Globalization.CultureInfo.InvariantCulture),top=Double.Parse(row[2],System.Globalization.CultureInfo.InvariantCulture);
  int x=(int)Math.Floor((sourceX-left)*.32+Double.Parse(row[5],System.Globalization.CultureInfo.InvariantCulture));
  int y=(int)Math.Floor((sourceY-top)*.32+Double.Parse(row[6],System.Globalization.CultureInfo.InvariantCulture));
  using(Bitmap frame=new Bitmap("assets/throw/"+name))using(Bitmap raw=new Bitmap("assets/source-samples/"+name+"-"+sourceX+"-"+sourceY+".png")) {
   Color actual=frame.GetPixel(x,y);Check(actual.A==255,"source landmark opaque: "+name+" "+sourceX+","+sourceY);
   bool original=false;for(int yy=sourceY-4;yy<=sourceY+4;yy++)for(int xx=sourceX-4;xx<=sourceX+4;xx++) {
    Color c=raw.GetPixel(xx-sourceX+4,yy-sourceY+4);if(c.R==actual.R&&c.G==actual.G&&c.B==actual.B)original=true;
   }
   Check(original,"retains original local RGB: "+name);
  }
 }
 static void Main()
 {
  foreach(int width in new int[]{132,176,242}) {
   int height=width*200/220;Check(PetHitArea.Contains(width,height,width/2,height/2),"center hit");
   Check(!PetHitArea.Contains(width,height,0,0)&&!PetHitArea.Contains(width,height,width-1,height-1),"corners pass through");
   RectangleF circle=PetHitArea.Circle(width,height);Check(circle.Width==circle.Height,"true circle");
   using(Bitmap image=new Bitmap(width,height,PixelFormat.Format32bppPArgb)) {
    PetHitArea.MakeClickable(image);Check(image.GetPixel(width/2,height/2).A>0,"transparent center remains clickable");
    Check(image.GetPixel(0,0).A==0,"outside untouched");Check(image.GetPixel(width/2,height/2).A<=1,"hit surface virtually invisible");
   }
  }
  string[] files=System.IO.Directory.GetFiles("assets/throw","*.png");Check(files.Length==78,"throw frame count");
  foreach(string path in files)using(Bitmap image=new Bitmap(path)) {
   Check(image.GetPixel(0,0).A==0&&image.GetPixel(219,199).A==0,"clear corners");
   for(int y=112;y<=122;y++)for(int x=99;x<=116;x++)Check(image.GetPixel(x,y).A==255,"opaque core: "+path+" "+x+","+y);
  }
  SourcePoint("frame-015.png",550,580);SourcePoint("frame-052.png",700,570);
  SourcePoint("frame-073.png",540,425);SourcePoint("frame-078.png",480,415);SourcePoint("frame-078.png",700,590);
  Console.WriteLine("PASS: independent source015/052toes and073/078nose+foot opaque/local-original-RGB regressions");
  Console.WriteLine("PASS: circular hit geometry at3sizes,alpha1 hit surface,pass-through corners,78 opaque sprite cores/transparent corners");
 }
}
