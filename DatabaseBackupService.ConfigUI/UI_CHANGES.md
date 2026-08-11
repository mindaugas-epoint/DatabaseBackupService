# ConfigUI Updates - Email Notification Feature

## What Was Added

### New UI Section: "Email Notifications (Optional)"

Located at the bottom of the configuration window, this new section includes:

```
┌─ Email Notifications (Optional) ────────────────────────────────────┐
│                                                                       │
│  Gmail Address:      [yourname@gmail.com                        ]    │
│                                                                       │
│  App Password:       [****************                           ]    │
│                                                                       │
│  Send Alerts To:     [admin@yourcompany.com                      ]    │
│                                                                       │
│  Get email alerts when database backups fail.                        │
│                                         How to get App Password       │
│                                         (clickable link)              │
└───────────────────────────────────────────────────────────────────────┘

                            [Test Connection]  [Save]
```

## UI Elements

### 1. Gmail Address Field
- **Type**: Text input
- **Placeholder**: "yourname@gmail.com"
- **Validation**: Must end with @gmail.com

### 2. App Password Field
- **Type**: Password input (masked)
- **Placeholder**: "16-character app password"
- **Validation**: Required if email is configured

### 3. Send Alerts To Field
- **Type**: Text input
- **Placeholder**: "admin@yourcompany.com"
- **Validation**: Must be valid email format

### 4. Information Label
- **Text**: "Get email alerts when database backups fail."
- **Purpose**: Explains the feature

### 5. Help Link
- **Text**: "How to get App Password"
- **Action**: Opens https://myaccount.google.com/apppasswords
- **Purpose**: Quick access to Google App Password creation

## Window Size Changes

- **Old Size**: 484 x 788 pixels
- **New Size**: 484 x 950 pixels
- **Height Increase**: +162 pixels (to accommodate email section)

## Control Positions

### Email GroupBox
- **Location**: 12, 740 (x, y from top-left)
- **Size**: 460 x 155 (width x height)

### Buttons (Adjusted)
- **Test Connection**: 232, 908 (moved down from 747)
- **Save**: 367, 908 (moved down from 747)

## Updated Files

### 1. MainForm.Designer.cs
- Added new GroupBox: `groupBoxEmail`
- Added controls: `textBoxEmailSender`, `textBoxEmailPassword`, `textBoxEmailRecipient`
- Added labels: `labelEmailSender`, `labelEmailPassword`, `labelEmailRecipient`, `labelEmailInfo`
- Added link: `linkLabelGmailSetup`
- Updated form height: 950px
- Updated button positions

### 2. MainForm.cs
- Added `LoadConfigToUI` logic for email fields
- Added `ButtonSave_Click` logic to save email fields
- Added `LinkLabelGmailSetup_LinkClicked` event handler
- Added `IsValidEmail` helper method
- Updated `ValidateInput` with email validation

### 3. RegistryConfigManager.cs
- Added `BackupConfig` properties: `EmailSenderAddress`, `EmailSenderPassword`, `EmailRecipientAddress`
- Added `LoadConfig` logic to decrypt and load email password
- Added `SaveConfig` logic to encrypt and save email password

### 4. BackupConfig class
- Added 3 new properties for email configuration
- All properties default to empty strings

## Features

### Validation Rules
1. **Optional but Complete**: If any email field is filled, all three must be filled
2. **Gmail Only**: Sender must be a Gmail address (@gmail.com)
3. **Valid Email**: Recipient must be a valid email format
4. **App Password Required**: If email is configured, app password cannot be empty

### Security
- Email passwords are encrypted using Windows DPAPI
- Stored in registry as binary (encrypted) data
- Password fields show asterisks (*) instead of plain text
- Same encryption method used for database and Azure passwords

### User Experience
- Clear placeholder text for guidance
- Helpful link to Gmail App Password creation page
- Informative validation messages
- Optional configuration - doesn't interfere with existing functionality

## Backend Integration

The ConfigUI saves email configuration to registry:
- **Key**: `HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService`
- **Values**: 
  - `EmailSenderAddress` (String)
  - `EmailSenderPassword` (Binary, encrypted)
  - `EmailRecipientAddress` (String)

The Database Backup Service reads these values and uses them to send email notifications when backups fail.

## Complete Configuration Flow

1. **User opens ConfigUI**
2. **Scrolls to "Email Notifications" section**
3. **Clicks "How to get App Password" link** (if needed)
   - Opens Google App Passwords page
   - User creates app password
   - Copies 16-character password
4. **Returns to ConfigUI**
5. **Fills in three fields**:
   - Gmail Address
   - App Password (paste from clipboard)
   - Recipient Email
6. **Clicks Save**
7. **Configuration validates and saves**
8. **Service reads configuration on startup**
9. **When backup fails, email is sent automatically**

## Visual Hierarchy

```
Database Connection (GroupBox)
    ├─ Database Type
    ├─ Server Name
    ├─ Port
    ├─ Database Name
    ├─ User Name
    └─ Password

Backup Settings (GroupBox)
    ├─ Backup Schedule
    ├─ Multiple Backups Per Day (Checkbox)
    ├─ Start/End/Interval Times
    └─ Backup Path

Azure Blob Storage (GroupBox)
    ├─ Enable Azure (Checkbox)
    ├─ Authentication Method (Radio Buttons)
    ├─ Connection String / SAS Token
    ├─ Storage Account / Container
    └─ Test Button

Email Notifications (GroupBox) ← NEW!
    ├─ Gmail Address
    ├─ App Password
    ├─ Recipient Email
    ├─ Info Label
    └─ Help Link

Buttons
    ├─ Test Connection
    └─ Save
```

## Color Scheme & Styling

- **GroupBox Border**: Standard Windows theme
- **Labels**: Default system font, standard color
- **TextBoxes**: Standard Windows input style
- **Link**: Blue, underlined (standard hyperlink style)
- **Password Fields**: Black dots/asterisks
- **Info Label**: Gray text (GrayText system color)

## Accessibility

- All controls have proper **Tab Index**
- Labels are associated with their controls
- Keyboard navigation works properly
- Link can be activated with Enter key
- Validation messages use MessageBox for screen reader compatibility

## Testing Checklist

✅ UI renders correctly at new size (484 x 950)  
✅ Email fields appear in correct positions  
✅ Placeholders show helpful examples  
✅ Password field masks input  
✅ Help link opens browser to Google  
✅ Validation works for all scenarios  
✅ Save button persists email config to registry  
✅ Load retrieves email config from registry  
✅ Encrypted passwords are properly stored/retrieved  
✅ Optional nature doesn't break existing functionality  

## Next Steps for Users

After configuring email in the UI:
1. Save the configuration
2. Restart the Database Backup Service (if running)
3. Service will automatically use email configuration
4. Email will be sent only when backups fail
5. Check spam folder if email not received
