using System;
using System.Windows.Forms;

namespace ChatBox.Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Hiển thị form đăng nhập
            var loginForm = new Forms.frmLogin();
            var result = loginForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 2. Đăng nhập thành công → mở form chat
                Application.Run(new Forms.frmChat(
                    loginForm.TcpService,
                    loginForm.LoggedInUserId,
                    loginForm.LoggedInDisplayName));
            }
        }
    }
}
