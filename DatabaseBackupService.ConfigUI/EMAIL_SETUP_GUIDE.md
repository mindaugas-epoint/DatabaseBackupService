# ConfigUI - Gmail Email Notification Setup

The Database Backup Service Configuration UI now includes support for configuring Gmail email notifications for backup failures.

## Email Notification Configuration

### UI Location
The email configuration section is located at the bottom of the Configuration UI under **"Email Notifications (Optional)"**.

### Fields

#### 1. Gmail Address
- **Required**: Only if you want email notifications
- **Format**: Must be a valid Gmail address (ending with @gmail.com)
- **Example**: `yourname@gmail.com`
- This is the Gmail account that will SEND the notification emails

#### 2. App Password
- **Required**: Only if you want email notifications
- **Type**: Gmail App Password (16-character password)
- **Important**: This is NOT your regular Gmail password
- **How to get**: Click the "How to get App Password" link in the UI, or see instructions below

#### 3. Send Alerts To
- **Required**: Only if you want email notifications
- **Format**: Any valid email address
- **Example**: `admin@yourcompany.com` or `it-team@company.com`
- This is the email address that will RECEIVE backup failure notifications

### Optional Configuration
All three email fields are optional. However, if you fill in any one field, you must fill in all three fields for the email notifications to work.

If you leave all three fields empty, no email notifications will be sent (service will continue to work normally).

## How to Get Gmail App Password

### Prerequisites
1. You must have 2-Factor Authentication (2FA) enabled on your Google account
2. You must use a Gmail account (@gmail.com)

### Steps

1. **Enable 2-Factor Authentication**
   - Go to: https://myaccount.google.com/security
   - Find "2-Step Verification" section
   - Follow the prompts to enable 2FA if not already enabled

2. **Create App Password**
   - Click the link in the ConfigUI: "How to get App Password"
   - Or manually go to: https://myaccount.google.com/apppasswords
   - If prompted, sign in to your Google account
   - Select:
     - **App**: Mail
     - **Device**: Windows Computer (or select "Other" and name it "Database Backup Service")
   - Click **Generate**
   - Google will display a 16-character password (e.g., `abcd efgh ijkl mnop`)

3. **Copy the App Password**
   - Copy the entire 16-character password
   - You can include or exclude the spaces - both work
   - Click **Done**

4. **Paste in ConfigUI**
   - Return to the Database Backup Service Configuration UI
   - Paste the app password into the "App Password" field
   - The password will be masked with asterisks for security

## Security

### Password Encryption
- The Gmail App Password is encrypted before being saved to the Windows Registry
- Encryption uses Windows DPAPI (Data Protection API)
- The password is encrypted for the current user only
- The service must run under the same user account that configured the settings

### Best Practices
1. **Use a dedicated Gmail account** for sending notifications (optional but recommended)
2. **Regularly rotate** app passwords for security
3. **Revoke unused app passwords** from your Google account settings
4. **Never share** your app password or commit it to source control

### Password Management
- **View passwords**: Google App Passwords can be viewed in your Google Account settings
- **Revoke access**: You can revoke app passwords at any time from https://myaccount.google.com/apppasswords
- **If compromised**: Simply delete the app password in your Google account and create a new one

## Validation

The ConfigUI validates email settings when you click **Save**:

✅ **Gmail Address** must end with @gmail.com  
✅ **App Password** must not be empty if email is configured  
✅ **Recipient** must be a valid email format  
✅ **All or nothing** - if any field is filled, all three must be filled  

## Testing Email Configuration

After saving the configuration:

1. **Method 1: Trigger a Backup Failure**
   - Temporarily change the backup path to an invalid location
   - Wait for the next scheduled backup
   - Check the recipient email for the failure notification
   - Don't forget to fix the backup path!

2. **Method 2: Check Service Logs**
   - Start the Database Backup Service
   - Check the logs for any email configuration warnings
   - If configured correctly, you'll see: "Email notification is configured"
   - If not configured: "Email notification is not configured. Skipping email notification."

## Troubleshooting

### "Authentication Failed"
- **Cause**: Incorrect app password or username
- **Solution**: 
  - Verify 2FA is enabled on your Google account
  - Generate a new app password
  - Make sure you're using the app password, NOT your regular Gmail password
  - Check that the Gmail address is correct

### "Email notification is not configured"
- **Cause**: One or more email fields are empty
- **Solution**: Ensure all three email fields are filled in the ConfigUI

### "Failed to send email notification"
- **Cause**: Network issues or firewall blocking SMTP
- **Solution**: 
  - Check your internet connection
  - Verify firewall allows outbound connections on port 587
  - Check service logs for detailed error message

### No Email Received
- **Cause**: Email might be in spam folder
- **Solution**:
  - Check the recipient's spam/junk folder
  - Add the sender Gmail address to your contacts
  - Verify the recipient email address is correct in the ConfigUI

## Example Configuration

```
Gmail Address:     backup-alerts@gmail.com
App Password:      abcdefghijklmnop
Send Alerts To:    it-admin@yourcompany.com
```

After clicking **Save**, you'll see:
- "Configuration saved successfully!"

When a backup fails, an email like this will be sent:

**Subject**: Database Backup Service - Backup Failure Alert

**Body**:
```
⚠️ Database Backup Failure Alert

Time: 2024-04-17 14:30:00
Failed Backups: 1

Error Details:
Database: MyDatabase
Error: Access denied to path 'C:\InvalidPath'

This is an automated message from the Database Backup Service.
Please investigate and resolve the backup failures as soon as possible.
```

## Email Features

The notification email includes:
- ✉️ Professional HTML formatting
- 🕒 Timestamp of the failure
- 📊 Number of failed backups
- 🗃️ Database name(s) affected
- ⚠️ Detailed error messages
- 🎨 Color-coded alerts (red for errors)

## Gmail Limitations

**Free Gmail Account Limits**:
- Maximum 500 emails per day
- Maximum 100-150 emails per hour
- These limits are more than sufficient for backup failure notifications

**If Limits Are Exceeded**:
- Gmail will temporarily block sending
- Service logs will show the error
- Emails will not be lost (just not sent)
- Wait 24 hours for limits to reset

## Additional Notes

- Email notifications are sent **only when backups fail**
- No emails are sent for successful backups
- Multiple backup failures in one run are grouped into a single email
- The email service uses **MailKit** library (industry standard, better than deprecated SmtpClient)
- SMTP Settings: `smtp.gmail.com:587` with StartTLS encryption

## Support

For more information about email notifications, see:
- `DatabaseBackupService/EmailService/README.md` - Technical implementation details
- `DatabaseBackupService/EmailService/QUICKSTART.md` - Quick setup via PowerShell

For Gmail App Password help:
- Google Support: https://support.google.com/accounts/answer/185833
