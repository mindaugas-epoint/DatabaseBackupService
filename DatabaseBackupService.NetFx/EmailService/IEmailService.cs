using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseBackupService.NetFx.EmailService
{
    public interface IEmailService
    {
        Task SendBackupFailureNotificationAsync(List<(string databaseName, string error)> errors);
    }
}
