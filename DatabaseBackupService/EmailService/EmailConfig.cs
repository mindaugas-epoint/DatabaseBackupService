namespace DatabaseBackupService.EmailService
{
    public class EmailConfig
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
        public string SenderName { get; set; } = "Database Backup Service";
        public string RecipientEmail { get; set; } = "";
        public string RecipientName { get; set; } = "";

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(SenderEmail) &&
                   !string.IsNullOrEmpty(SenderPassword) &&
                   !string.IsNullOrEmpty(RecipientEmail);
        }
    }
}
