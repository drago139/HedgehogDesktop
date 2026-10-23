using System;
using System.Drawing;
using System.Windows.Forms;
internal sealed class PetApplication : ApplicationContext
{
    readonly PetRoster<PetForm> pets=new PetRoster<PetForm>();
    readonly NotifyIcon tray=new NotifyIcon();
    readonly ContextMenuStrip menu=new ContextMenuStrip();
    bool closing;
    int ordinal;
    internal PetApplication()
    {
        menu.Font=new Font("Segoe UI",10);
        menu.Items.Add("Add hedgehog",null,delegate {AddPet();});
        menu.Items.Add("Bring hedgehog to cursor",null,delegate {PetForm[] active=pets.Snapshot();if(active.Length>0)active[0].Summon();});
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Remove all hedgehogs",null,delegate {CloseAll();});
        using(Bitmap bitmap=new Bitmap(32,32))
        {
            using(Graphics graphics=Graphics.FromImage(bitmap)) {graphics.Clear(Color.Transparent);graphics.DrawImage(PetFrames.Frames[0],new Rectangle(0,0,32,32));}
            IntPtr handle=bitmap.GetHicon();
            try {using(Icon icon=Icon.FromHandle(handle))tray.Icon=(Icon)icon.Clone();}finally {Native.DestroyIcon(handle);}
        }
        tray.ContextMenuStrip=menu;tray.DoubleClick+=delegate {AddPet();};
        AddPet();tray.Visible=true;
    }
    void AddPet()
    {
        if(closing)return;
        PetForm pet=new PetForm(AddPet,CloseAll,ordinal++);
        pets.Add(pet);
        pet.FormClosed+=delegate {pets.Remove(pet);UpdateTray();if(pets.Count==0&&!closing)ExitThread();};
        try {pet.Show();UpdateTray();}
        catch {pets.Remove(pet);pet.Dispose();throw;}
    }
    void UpdateTray() {tray.Text="Hedgehogs: "+pets.Count+" - right-click for menu";}
    void CloseAll()
    {
        if(closing)return;closing=true;
        try {foreach(PetForm pet in pets.Snapshot())pet.Close();}
        finally {ExitThread();}
    }
    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            closing=true;foreach(PetForm pet in pets.Snapshot()) {pet.Close();pet.Dispose();}
            tray.Visible=false;if(tray.Icon!=null)tray.Icon.Dispose();tray.Dispose();menu.Dispose();PetFrames.Release();
        }
        base.Dispose(disposing);
    }
}
