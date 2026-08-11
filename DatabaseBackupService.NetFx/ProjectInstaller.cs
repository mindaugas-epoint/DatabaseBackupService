using System.ComponentModel;
using System.ServiceProcess;

namespace DatabaseBackupService.NetFx
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.ServiceName = "DatabaseBackupService";
            serviceInstaller.DisplayName = "Database Backup Service (.NET Framework)";
            serviceInstaller.Description = "Automated database backup service for SQL Server and MySQL (.NET Framework 4.8)";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
