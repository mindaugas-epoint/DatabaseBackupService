namespace DatabaseBackupService.NetFx.EmailService
{
    public class EmailConfig
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SenderName { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }

        public EmailConfig()
        {
            SmtpServer = "smtp.gmail.com";
            SmtpPort = 587;
            SenderEmail = "";
            SenderPassword = "";
            SenderName = "Database Backup Service";
            RecipientEmail = "";
            RecipientName = "";
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(SenderEmail) &&
                   !string.IsNullOrEmpty(SenderPassword) &&
                   !string.IsNullOrEmpty(RecipientEmail);
        }
    }
}
