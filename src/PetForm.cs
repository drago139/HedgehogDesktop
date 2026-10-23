using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

internal sealed class PetForm : Form
{
    readonly List<Bitmap> frames=PetFrames.Frames;
    readonly Walker walker=new Walker(Guid.NewGuid().GetHashCode());
    readonly ThrowMotion thrown=new ThrowMotion();
    readonly FlickGesture flick=new FlickGesture();
    bool drawnThrow;
    readonly Timer timer=new Timer();
    readonly Stopwatch clock=new Stopwatch();
    readonly FrameClock animation=new FrameClock(75,25);
    readonly ContextMenuStrip menu=new ContextMenuStrip();
    readonly Action addPet,closeAll;
    readonly string settingsPath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"settings.xml");
    readonly uint summonMessage=Native.RegisterWindowMessage("HedgehogDesktop.Summon.20261022");
    ToolStripMenuItem pauseItem;
    Rectangle area;
    bool dragging;
    Point dragOffset;
    double lastTime,hiddenUntil;
    int drawnFrame=-1,petWidth=176;
    bool drawnRight;

    int petHeight { get {return petWidth*200/220;} }

    protected override bool ShowWithoutActivation {get {return true;} }
    protected override CreateParams CreateParams
    {
        get {CreateParams p=base.CreateParams;p.ExStyle|=0x00080000|0x00000080|0x08000000;p.Style&=~0x00C00000;return p;}
    }

    internal PetForm(Action add,Action removeAll,int ordinal)
    {
        addPet=add;closeAll=removeAll;
        Text="Hedgehog Desktop";Name="HedgehogDesktop";AccessibleName="Desktop hedgehog";
        FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;TopMost=true;
        StartPosition=FormStartPosition.Manual;AutoScaleMode=AutoScaleMode.None;
        DoubleBuffered=false;

        LoadSettings();
        area=Screen.PrimaryScreen.WorkingArea;
        walker.Place(area.Right-petWidth-90-(ordinal%5)*(petWidth+12),area.Bottom-petHeight-8-(ordinal/5%3)*(petHeight+12),area,petWidth,petHeight);
        ClientSize=new Size(petWidth,petHeight);Location=new Point((int)walker.X,(int)walker.Y);
        BuildMenu();ContextMenuStrip=menu;
        timer.Interval=16;timer.Tick+=Tick;
        SystemEventsSubscribe();
    }

    ToolStripMenuItem Add(string title,EventHandler action)
    {ToolStripMenuItem item=new ToolStripMenuItem(title);if(action!=null)item.Click+=action;menu.Items.Add(item);return item;}
    void BuildMenu()
    {
        menu.Font=new Font("Segoe UI",10);
        Add("My hedgehog",null).Enabled=false;
        Add("Add hedgehog",delegate {addPet();});
        menu.Items.Add(new ToolStripSeparator());
        pauseItem=Add("Pause",delegate {thrown.Stop();walker.Paused=!walker.Paused;SaveSettings();});
        Add("Bring to cursor",delegate {Summon();});
        ToolStripMenuItem speed=Add("Speed",null);
        foreach(int value in new int[]{30,55,95})
        {
            int selected=value;ToolStripMenuItem item=new ToolStripMenuItem(value==30?"Slow":value==55?"Normal":"Fast");
            item.Tag=value;item.Click+=delegate {walker.Speed=selected;SaveSettings();};speed.DropDownItems.Add(item);
        }
        ToolStripMenuItem size=Add("Size",null);
        foreach(int value in new int[]{132,176,242})
        {
            int selected=value;ToolStripMenuItem item=new ToolStripMenuItem(value==132?"Small":value==176?"Medium":"Large");
            item.Tag=value;item.Click+=delegate {thrown.Stop();petWidth=selected;ClientSize=new Size(petWidth,petHeight);walker.Place(walker.X,walker.Y,area,petWidth,petHeight);drawnFrame=-1;DrawPet();SaveSettings();};size.DropDownItems.Add(item);
        }
        ToolStripMenuItem route=Add("Walking area",null);
        ToolStripMenuItem all=new ToolStripMenuItem("Entire screen");all.Tag=false;
        all.Click+=delegate {thrown.Stop();walker.BottomOnly=false;walker.Place(walker.X,walker.Y,area,petWidth,petHeight);SaveSettings();};route.DropDownItems.Add(all);
        ToolStripMenuItem bottom=new ToolStripMenuItem("Bottom edge only");bottom.Tag=true;
        bottom.Click+=delegate {thrown.Stop();walker.BottomOnly=true;walker.Place(walker.X,walker.Y,area,petWidth,petHeight);DrawPet();SaveSettings();};route.DropDownItems.Add(bottom);
        ToolStripMenuItem monitor=Add("Move to next monitor",delegate {
            thrown.Stop();Screen[] screens=Screen.AllScreens;int index=0;
            for(int i=0;i<screens.Length;i++) if(screens[i].WorkingArea==area) index=i;
            area=screens[(index+1)%screens.Length].WorkingArea;
            walker.Place(area.Right-petWidth-40,area.Bottom-petHeight,area,petWidth,petHeight);DrawPet();
        });
        monitor.Enabled=Screen.AllScreens.Length>1;
        Add("Hide for 30 seconds",delegate {hiddenUntil=clock.Elapsed.TotalSeconds+30;Hide();});
        menu.Items.Add(new ToolStripSeparator());
        Add("Drag quickly and release to throw",null).Enabled=false;
        Add("Remove this hedgehog",delegate {Close();});
        Add("Remove all hedgehogs",delegate {closeAll();});
        menu.Opening+=delegate {
            pauseItem.Text=walker.Paused?"Resume walking":"Pause";pauseItem.Checked=walker.Paused;
            foreach(ToolStripMenuItem i in speed.DropDownItems) i.Checked=(int)i.Tag==(int)walker.Speed;
            foreach(ToolStripMenuItem i in size.DropDownItems) i.Checked=(int)i.Tag==petWidth;
            foreach(ToolStripMenuItem i in route.DropDownItems) i.Checked=(bool)i.Tag==walker.BottomOnly;
        };
    }

    protected override void OnShown(EventArgs e) {base.OnShown(e);DrawPet();clock.Start();timer.Start();}
    protected override void OnPaintBackground(PaintEventArgs e) {}

    void Tick(object sender,EventArgs e)
    {
        double now=clock.Elapsed.TotalSeconds,elapsed=Math.Max(0,now-lastTime),dt=Math.Min(0.1,elapsed);lastTime=now;
        if(hiddenUntil>0)
        {
            if(now<hiddenUntil) return;
            hiddenUntil=0;Show();drawnFrame=-1;
        }
        if(dragging||menu.Visible) return;
        Rectangle hover=Bounds;hover.Inflate(12,12);
        Point mouse=PointToClient(Cursor.Position);
        bool hovering=PetHitArea.Contains(ClientSize.Width,ClientSize.Height,mouse.X,mouse.Y);
        bool wasThrown=thrown.Active;
        if(wasThrown)
        {
            thrown.Step(elapsed,area,petWidth,petHeight);
            if(!thrown.Active)walker.Place(thrown.X,thrown.Y,area,petWidth,petHeight);
        }
        else if(!hovering)walker.Step(dt,area,petWidth,petHeight);
        animation.Advance(elapsed,walker.Paused);
        if(CurrentFrame!=drawnFrame||walker.FacingRight!=drawnRight||drawnThrow!=thrown.Active) DrawPet();
        else Native.SetWindowPos(Handle,IntPtr.Zero,(int)Math.Round(RenderX),(int)Math.Round(RenderY),0,0,0x0001|0x0004|0x0010);
    }

    double RenderX {get {return thrown.Active?thrown.X:walker.X;}}
    double RenderY {get {return thrown.Active?thrown.Y:walker.Y;}}
    int CurrentFrame {get {return thrown.Active?(int)thrown.Phase%PetFrames.ThrowFrames.Count:animation.Frame;}}
    void DrawPet()
    {
        if(!IsHandleCreated) return;
        using(Bitmap output=new Bitmap(petWidth,petHeight,PixelFormat.Format32bppPArgb))
        using(Graphics graphics=Graphics.FromImage(output))
        {
            graphics.Clear(Color.Transparent);graphics.CompositingMode=CompositingMode.SourceCopy;
            graphics.InterpolationMode=InterpolationMode.NearestNeighbor;graphics.PixelOffsetMode=PixelOffsetMode.Half;
            if(!thrown.Active&&walker.FacingRight) {graphics.TranslateTransform(petWidth,0);graphics.ScaleTransform(-1,1);}
            graphics.DrawImage(thrown.Active?PetFrames.ThrowFrames[CurrentFrame]:frames[CurrentFrame],new Rectangle(0,0,petWidth,petHeight),0,0,220,200,GraphicsUnit.Pixel);
            PetHitArea.MakeClickable(output);
            Native.Display(Handle,output,(int)Math.Round(RenderX),(int)Math.Round(RenderY));
        }
        drawnFrame=CurrentFrame;drawnRight=walker.FacingRight;drawnThrow=thrown.Active;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if(e.Button==MouseButtons.Left) {
            double x=RenderX,y=RenderY;thrown.Stop();bool bottom=walker.BottomOnly;walker.BottomOnly=false;
            walker.Place(x,y,area,petWidth,petHeight);walker.BottomOnly=bottom;
            dragging=true;dragOffset=e.Location;flick.Begin(clock.Elapsed.TotalSeconds,Cursor.Position.X,Cursor.Position.Y);Capture=true;
        }

    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if(dragging)
        {
            Point cursor=Cursor.Position;flick.Move(clock.Elapsed.TotalSeconds,cursor.X,cursor.Y);area=Screen.FromPoint(cursor).WorkingArea;
            walker.Place(cursor.X-dragOffset.X,cursor.Y-dragOffset.Y,area,petWidth,petHeight);DrawPet();
        }
    }
    protected override void OnMouseUp(MouseEventArgs e) {base.OnMouseUp(e);if(e.Button==MouseButtons.Left&&dragging) {
        PointF velocity=flick.Release(clock.Elapsed.TotalSeconds,Cursor.Position.X,Cursor.Position.Y);
        dragging=false;Capture=false;
        if(velocity!=PointF.Empty) {walker.Paused=false;thrown.Launch(walker.X,walker.Y,velocity.X,velocity.Y);DrawPet();}
    }}
    protected override void OnMouseCaptureChanged(EventArgs e) {base.OnMouseCaptureChanged(e);if(!Capture) dragging=false;}
    protected override void OnMouseDoubleClick(MouseEventArgs e) {base.OnMouseDoubleClick(e);if(e.Button==MouseButtons.Left) {thrown.Stop();walker.Paused=!walker.Paused;SaveSettings();}}

    internal void Summon()
    {
        thrown.Stop();hiddenUntil=0;Point p=Cursor.Position;area=Screen.FromPoint(p).WorkingArea;
        walker.Place(p.X-petWidth/2,p.Y-petHeight/2,area,petWidth,petHeight);Show();DrawPet();
    }
    protected override void WndProc(ref Message message)
    {
        if(message.Msg==0x0084) {
            long point=message.LParam.ToInt64();int x=(short)(point&65535),y=(short)((point>>16)&65535);
            Point local=PointToClient(new Point(x,y));
            message.Result=new IntPtr(PetHitArea.Contains(ClientSize.Width,ClientSize.Height,local.X,local.Y)?1:-1);return;
        }
        if(message.Msg==summonMessage) {addPet();return;}
        if(message.Msg==0x0021) {message.Result=new IntPtr(3);return;}
        base.WndProc(ref message);
    }

    void LoadSettings()
    {
        try
        {
            XElement root=XElement.Load(settingsPath);
            int width=(int?)root.Element("Width")??176;
            petWidth=width==132||width==176||width==242?width:176;
            double speed=(double?)root.Element("Speed")??55;
            walker.Speed=speed==30||speed==55||speed==95?speed:55;
            walker.BottomOnly=(bool?)root.Element("BottomOnly")??false;
        }
        catch { }
    }
    void SaveSettings()
    {
        try {new XElement("Hedgehog",new XElement("Width",petWidth),new XElement("Speed",walker.Speed),new XElement("BottomOnly",walker.BottomOnly)).Save(settingsPath);}
        catch { }
    }
    void SystemEventsSubscribe() {Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;}
    void DisplayChanged(object sender,EventArgs e)
    {
        if(IsDisposed)return;
        BeginInvoke((MethodInvoker)delegate {area=Screen.FromPoint(Location).WorkingArea;walker.Place(walker.X,walker.Y,area,petWidth,petHeight);DrawPet();});
    }
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        timer.Stop();timer.Dispose();SaveSettings();
        Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;
        menu.Dispose();
        base.OnFormClosed(e);
    }
}
