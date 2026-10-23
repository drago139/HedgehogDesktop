using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
internal static class CreateIcon
{
 static GraphicsPath Rounded(float x,float y,float width,float height,float radius)
 {
  GraphicsPath p=new GraphicsPath();float d=radius*2;
  p.AddArc(x,y,d,d,180,90);p.AddArc(x+width-d,y,d,d,270,90);
  p.AddArc(x+width-d,y+height-d,d,d,0,90);p.AddArc(x,y+height-d,d,d,90,90);p.CloseFigure();return p;
 }
 static Bitmap Render(Bitmap pet,int size)
 {
  Bitmap output=new Bitmap(size,size,PixelFormat.Format32bppArgb);
  using(Graphics g=Graphics.FromImage(output))
  {
   g.Clear(Color.Transparent);g.SmoothingMode=SmoothingMode.AntiAlias;
   using(GraphicsPath path=Rounded(size*.025f,size*.025f,size*.95f,size*.95f,size*.21f))
   using(Brush fill=new SolidBrush(Color.FromArgb(219,235,225)))g.FillPath(fill,path);
   using(Brush shadow=new SolidBrush(Color.FromArgb(38,43,67,53)))g.FillEllipse(shadow,size*.16f,size*.78f,size*.69f,size*.09f);
   g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
   g.DrawImage(pet,new RectangleF(size*.07f,size*.13f,size*.88f,size*.72f),new RectangleF(8,20,202,160),GraphicsUnit.Pixel);
  }
  return output;
 }
 static void Main(string[] args)
 {
  int[] sizes={16,24,32,48,64,128,256};List<byte[]> images=new List<byte[]>();
  using(Bitmap pet=new Bitmap(args[0]))foreach(int size in sizes)using(Bitmap icon=Render(pet,size))using(MemoryStream data=new MemoryStream())
  {icon.Save(data,ImageFormat.Png);images.Add(data.ToArray());if(size==256)icon.Save(args[2],ImageFormat.Png);}
  using(BinaryWriter writer=new BinaryWriter(File.Create(args[1])))
  {
   writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;
   for(int i=0;i<sizes.Length;i++) {writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}
   foreach(byte[] data in images)writer.Write(data);
  }
  for(int i=0;i<images.Count;i++)using(MemoryStream data=new MemoryStream(images[i]))using(Bitmap check=new Bitmap(data))if(check.Width!=sizes[i]||check.Height!=sizes[i])throw new Exception("Wrong icon PNG dimensions");
  Console.WriteLine("PASS: seven icon sizes16..256px;256px decode");
 }
}

