using System;
using System.Windows.Forms;

namespace ChatBox.Server
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.frmServer());
        }
    }
}
