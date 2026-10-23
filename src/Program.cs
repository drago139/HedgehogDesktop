using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        bool created;
        using(Mutex mutex=new Mutex(true,"Local\\HedgehogDesktop.20261022",out created))
        {
            if(!created)
            {
                IntPtr window=Native.FindWindow(null,"Hedgehog Desktop");
                if(window!=IntPtr.Zero)Native.PostMessage(window,Native.RegisterWindowMessage("HedgehogDesktop.Summon.20261022"),IntPtr.Zero,IntPtr.Zero);
                return;
            }
            Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException+=delegate(object sender,ThreadExceptionEventArgs args) {Report(args.Exception);Application.Exit();};
            try {using(PetApplication pets=new PetApplication())Application.Run(pets);}
            catch(Exception error) {Report(error);}
        }
    }
    static void Report(Exception error)
    {
        try {File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"error.log"),error.ToString());} catch { }
        MessageBox.Show("Could not start the hedgehog. See error.log next to the program for details.","Hedgehog",MessageBoxButtons.OK,MessageBoxIcon.Error);
    }
}
