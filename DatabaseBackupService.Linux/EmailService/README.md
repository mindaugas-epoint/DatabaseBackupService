# Gmail SMTP Email Notification Setup

This document explains how to configure Gmail SMTP for sending email notifications when database backups fail.

## Gmail Configuration

### 1. Enable 2-Factor Authentication (2FA)
1. Go to your Google Account settings: https://myaccount.google.com/
2. Navigate to **Security** → **2-Step Verification**
3. Follow the steps to enable 2FA

### 2. Create an App Password
1. Go to: https://myaccount.google.com/apppasswords
2. Select **App**: Mail
3. Select **Device**: Windows Computer (or Other)
4. Click **Generate**
5. **Copy the 16-character app password** (you'll need this for the configuration)

## Configuration via Registry

The email configuration is stored in the Windows Registry under:
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

You need to add the following registry values:

| Key Name | Type | Description | Example |
|----------|------|-------------|---------|
| `EmailSenderAddress` | String (REG_SZ) | Your Gmail address | `yourname@gmail.com` |
| `EmailSenderPassword` | Binary (REG_BINARY) | Encrypted app password | (Use ConfigUI to set) |
| `EmailRecipientAddress` | String (REG_SZ) | Email to receive notifications | `admin@company.com` |

### Important Notes:
- **Use App Password, NOT your Gmail password**
- The `EmailSenderPassword` should be the 16-character app password from Google
- The password is stored encrypted in the registry for security
- Gmail SMTP requires SSL/TLS on port 587

## Email Settings (Default)
- **SMTP Server**: smtp.gmail.com
- **SMTP Port**: 587
- **Security**: StartTLS
- **Sender Name**: Database Backup Service

## Testing the Configuration

After configuring the email settings:
1. Start the Database Backup Service
2. Trigger a backup failure (e.g., provide invalid backup path)
3. Check the recipient email for the failure notification

## Email Notification Content

When a backup fails, you'll receive an HTML-formatted email containing:
- Timestamp of the failure
- Number of failed backups
- Database name(s) that failed
- Detailed error messages for each failure

## Troubleshooting

### Common Issues:

**1. Authentication Failed**
- Ensure you're using an App Password, not your Gmail password
- Verify 2FA is enabled on your Google account
- Double-check the email address is correct

**2. Connection Timeout**
- Check your firewall settings (allow outbound port 587)
- Verify internet connectivity
- Try using port 465 with SSL (requires code modification)

**3. No Email Received**
- Check spam/junk folder
- Verify the recipient email address is correct
- Check service logs for email sending errors

**4. SSL/TLS Errors**
- Ensure your system has up-to-date root certificates
- Try updating .NET runtime to the latest version

## Alternative Email Providers

While this implementation is optimized for Gmail, you can modify the settings to use other SMTP providers:

| Provider | SMTP Server | Port | Notes |
|----------|-------------|------|-------|
| Gmail | smtp.gmail.com | 587 | Requires App Password |
| Outlook/Hotmail | smtp-mail.outlook.com | 587 | Requires App Password |
| SendGrid | smtp.sendgrid.net | 587 | Free tier: 100/day |
| Mailgun | smtp.mailgun.org | 587 | Free tier: 5,000/month |

To use a different provider, modify the `EmailConfig` class defaults in `EmailConfig.cs`.

## Security Considerations

- Email passwords are encrypted using Windows DPAPI (Data Protection API)
- Passwords are stored in the registry under the current user's context
- The service must run under the same user account that configured the settings
- Never commit email credentials to source control
- Regularly rotate app passwords for security

## Support

For issues or questions, please check the service logs at:
- Windows Event Viewer → Application logs
- Service log files (if file logging is enabled)
