namespace DatabaseBackupService.EmailService
{
    public interface IEmailService
    {
        Task SendBackupFailureNotificationAsync(List<(string databaseName, string error)> errors);
    }
}
