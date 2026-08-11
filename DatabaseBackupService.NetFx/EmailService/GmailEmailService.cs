using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace DatabaseBackupService.NetFx.EmailService
{
    public class GmailEmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private readonly Logger.ILogger _logger;

        public GmailEmailService(EmailConfig emailConfig, Logger.ILogger logger)
        {
            _emailConfig = emailConfig;
            _logger = logger;
        }

        public async Task SendBackupFailureNotificationAsync(List<(string databaseName, string error)> errors)
        {
            if (!_emailConfig.IsConfigured())
            {
                _logger.WriteLog("Warning", "Email notification is not configured. Skipping email notification.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailConfig.SenderName, _emailConfig.SenderEmail));
            message.To.Add(new MailboxAddress(_emailConfig.RecipientName, _emailConfig.RecipientEmail));
            message.Subject = "Database Backup Service - Backup Failure Alert";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = BuildEmailBody(errors);
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailConfig.SenderEmail, _emailConfig.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            _logger.WriteLog("Information", $"Backup failure notification email sent to {_emailConfig.RecipientEmail}");
        }

        private string BuildEmailBody(List<(string databaseName, string error)> errors)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var errorCount = errors.Count;

            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background-color: #d9534f; color: white; padding: 15px; border-radius: 5px; }}
        .content {{ margin-top: 20px; }}
        .error-item {{ background-color: #f8f9fa; border-left: 4px solid #d9534f; padding: 10px; margin: 10px 0; }}
        .database-name {{ font-weight: bold; color: #333; }}
        .error-message {{ color: #666; margin-top: 5px; font-family: monospace; font-size: 12px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #777; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h2>⚠️ Database Backup Failure Alert</h2>
    </div>
    <div class='content'>
        <p><strong>Time:</strong> {timestamp}</p>
        <p><strong>Failed Backups:</strong> {errorCount}</p>
        <hr>
        <h3>Error Details:</h3>";

            foreach (var (databaseName, error) in errors)
            {
                html += $@"
        <div class='error-item'>
            <div class='database-name'>Database: {databaseName}</div>
            <div class='error-message'>{System.Net.WebUtility.HtmlEncode(error)}</div>
        </div>";
            }

            html += @"
    </div>
    <div class='footer'>
        <p>This is an automated message from the Database Backup Service.</p>
        <p>Please investigate and resolve the backup failures as soon as possible.</p>
    </div>
</body>
</html>";

            return html;
        }
    }
}
