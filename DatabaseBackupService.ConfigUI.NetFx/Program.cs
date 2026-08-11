using System;
using System.Net;
using System.Windows.Forms;

namespace DatabaseBackupService.ConfigUI.NetFx
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Azure Storage requires TLS 1.2. On Windows 7 / .NET Framework, TLS 1.2 is not
            // enabled by default. Force it here so Azure connections succeed.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
