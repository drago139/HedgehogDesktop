using System;
using System.Drawing;
using System.Drawing.Drawing2D;
internal static class PetHitArea
{
    internal static RectangleF Circle(int width,int height)
    {
        float diameter=Math.Min(width,height)-4;return new RectangleF((width-diameter)/2,(height-diameter)/2,diameter,diameter);
    }
    internal static bool Contains(int width,int height,double x,double y)
    {
        RectangleF circle=Circle(width,height);double radius=circle.Width/2;
        double dx=x-circle.X-radius,dy=y-circle.Y-radius;return radius>0&&dx*dx+dy*dy<=radius*radius;
    }
    internal static void MakeClickable(Bitmap image)
    {
        // A layered window ignores fully transparent pixels before WM_NCHITTEST.
        // Alpha 1 supplies a virtually invisible hit surface independent of sprite holes.
        using(Graphics g=Graphics.FromImage(image))using(Brush fill=new SolidBrush(Color.FromArgb(1,0,0,0)))
        {g.CompositingMode=CompositingMode.SourceOver;g.FillEllipse(fill,Circle(image.Width,image.Height));}
    }
}
