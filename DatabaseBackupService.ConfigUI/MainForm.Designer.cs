namespace DatabaseBackupService.ConfigUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
collapsiblePanelDatabase = new CollapsiblePanel();
            textBoxPassword = new TextBox();
            labelPassword = new Label();
            textBoxUserName = new TextBox();
            labelUserName = new Label();
            textBoxDatabaseName = new TextBox();
            labelDatabaseName = new Label();
            textBoxServerName = new TextBox();
            labelServerName = new Label();
            textBoxPort = new TextBox();
            labelPort = new Label();
            comboBoxDatabaseType = new ComboBox();
            labelDatabaseType = new Label();
            collapsiblePanelSchedule = new CollapsiblePanel();
            buttonBrowse = new Button();
            textBoxBackupPath = new TextBox();
            labelBackupPath = new Label();
            checkBoxUseTimeWindow = new CheckBox();
            textBoxBackupInterval = new TextBox();
            labelBackupInterval = new Label();
            textBoxBackupEndTime = new TextBox();
            labelBackupEndTime = new Label();
            textBoxBackupStartTime = new TextBox();
            labelBackupStartTime = new Label();
            textBoxSchedule = new TextBox();
            labelSchedule = new Label();
            numericUpDownRetentionDays = new NumericUpDown();
            labelRetentionDays = new Label();
            numericUpDownMinBackupFiles = new NumericUpDown();
            labelMinBackupFiles = new Label();
            collapsiblePanelAzure = new CollapsiblePanel();
            buttonTestAzure = new Button();
            textBoxAzureBlobPrefix = new TextBox();
            labelAzureBlobPrefix = new Label();
            textBoxAzureContainerName = new TextBox();
            labelAzureContainerName = new Label();
            textBoxAzureSasToken = new TextBox();
            labelAzureSasToken = new Label();
            textBoxAzureStorageAccount = new TextBox();
            labelAzureStorageAccount = new Label();
            textBoxAzureConnectionString = new TextBox();
            labelAzureConnectionString = new Label();
            radioButtonSasToken = new RadioButton();
            radioButtonConnectionString = new RadioButton();
            checkBoxEnableAzure = new CheckBox();
            collapsiblePanelEmail = new CollapsiblePanel();
            linkLabelGmailSetup = new LinkLabel();
            labelEmailInfo = new Label();
            textBoxEmailRecipient = new TextBox();
            labelEmailRecipient = new Label();
            textBoxEmailPassword = new TextBox();
            labelEmailPassword = new Label();
            textBoxEmailSender = new TextBox();
            labelEmailSender = new Label();
            buttonSave = new Button();
            buttonTestConnection = new Button();
            folderBrowserDialog = new FolderBrowserDialog();
            panelMain = new Panel();
            ((System.ComponentModel.ISupportInitialize)numericUpDownRetentionDays).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinBackupFiles).BeginInit();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // collapsiblePanelDatabase
            // 
            collapsiblePanelDatabase.HeaderText = "Database Connection";
            collapsiblePanelDatabase.IsExpanded = true;
            collapsiblePanelDatabase.ExpandedHeight = 240;
            collapsiblePanelDatabase.Location = new Point(12, 12);
            collapsiblePanelDatabase.Name = "collapsiblePanelDatabase";
            collapsiblePanelDatabase.Size = new Size(560, 240);
            collapsiblePanelDatabase.TabIndex = 0;
            collapsiblePanelDatabase.ExpandedChanged += (s, e) => RepositionPanels();
            collapsiblePanelDatabase.ContentPanel.Controls.Add(textBoxPassword);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelPassword);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(textBoxUserName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelUserName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(textBoxDatabaseName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelDatabaseName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(textBoxServerName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelServerName);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(textBoxPort);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelPort);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(comboBoxDatabaseType);
            collapsiblePanelDatabase.ContentPanel.Controls.Add(labelDatabaseType);
            // 
            // textBoxPassword
            // 
            textBoxPassword.Location = new Point(120, 166);
            textBoxPassword.Name = "textBoxPassword";
            textBoxPassword.Size = new Size(400, 23);
            textBoxPassword.TabIndex = 11;
            textBoxPassword.UseSystemPasswordChar = true;
            // 
            // labelPassword
            // 
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(15, 169);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(60, 15);
            labelPassword.TabIndex = 10;
            labelPassword.Text = "Password:";
            // 
            // textBoxUserName
            // 
            textBoxUserName.Location = new Point(120, 134);
            textBoxUserName.Name = "textBoxUserName";
            textBoxUserName.Size = new Size(400, 23);
            textBoxUserName.TabIndex = 9;
            // 
            // labelUserName
            // 
            labelUserName.AutoSize = true;
            labelUserName.Location = new Point(15, 137);
            labelUserName.Name = "labelUserName";
            labelUserName.Size = new Size(68, 15);
            labelUserName.TabIndex = 8;
            labelUserName.Text = "User Name:";
            // 
            // textBoxDatabaseName
            // 
            textBoxDatabaseName.Location = new Point(120, 102);
            textBoxDatabaseName.Name = "textBoxDatabaseName";
            textBoxDatabaseName.Size = new Size(400, 23);
            textBoxDatabaseName.TabIndex = 7;
            // 
            // labelDatabaseName
            // 
            labelDatabaseName.AutoSize = true;
            labelDatabaseName.Location = new Point(15, 105);
            labelDatabaseName.Name = "labelDatabaseName";
            labelDatabaseName.Size = new Size(93, 15);
            labelDatabaseName.TabIndex = 6;
            labelDatabaseName.Text = "Database Name:";
            // 
            // textBoxServerName
            // 
            textBoxServerName.Location = new Point(120, 38);
            textBoxServerName.Name = "textBoxServerName";
            textBoxServerName.Size = new Size(400, 23);
            textBoxServerName.TabIndex = 3;
            // 
            // labelServerName
            // 
            labelServerName.AutoSize = true;
            labelServerName.Location = new Point(15, 41);
            labelServerName.Name = "labelServerName";
            labelServerName.Size = new Size(77, 15);
            labelServerName.TabIndex = 2;
            labelServerName.Text = "Server Name:";
            // 
            // textBoxPort
            // 
            textBoxPort.Location = new Point(120, 70);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(400, 23);
            textBoxPort.TabIndex = 5;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(15, 73);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(32, 15);
            labelPort.TabIndex = 4;
            labelPort.Text = "Port:";
            // 
            // comboBoxDatabaseType
            // 
            comboBoxDatabaseType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxDatabaseType.FormattingEnabled = true;
            comboBoxDatabaseType.Items.AddRange(new object[] { "mssql", "mysql" });
            comboBoxDatabaseType.Location = new Point(120, 6);
            comboBoxDatabaseType.Name = "comboBoxDatabaseType";
            comboBoxDatabaseType.Size = new Size(400, 23);
            comboBoxDatabaseType.TabIndex = 1;
            comboBoxDatabaseType.SelectedIndexChanged += ComboBoxDatabaseType_SelectedIndexChanged;
            // 
            // labelDatabaseType
            // 
            labelDatabaseType.AutoSize = true;
            labelDatabaseType.Location = new Point(15, 9);
            labelDatabaseType.Name = "labelDatabaseType";
            labelDatabaseType.Size = new Size(86, 15);
            labelDatabaseType.TabIndex = 0;
            labelDatabaseType.Text = "Database Type:";
            // 
            // collapsiblePanelSchedule
            // 
            collapsiblePanelSchedule.HeaderText = "Backup Settings";
            collapsiblePanelSchedule.IsExpanded = true;
            collapsiblePanelSchedule.ExpandedHeight = 290;
            collapsiblePanelSchedule.Location = new Point(12, 262);
            collapsiblePanelSchedule.Name = "collapsiblePanelSchedule";
            collapsiblePanelSchedule.Size = new Size(560, 290);
            collapsiblePanelSchedule.TabIndex = 1;
            collapsiblePanelSchedule.ExpandedChanged += (s, e) => RepositionPanels();
            collapsiblePanelSchedule.ContentPanel.Controls.Add(numericUpDownMinBackupFiles);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelMinBackupFiles);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(numericUpDownRetentionDays);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelRetentionDays);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(buttonBrowse);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(textBoxBackupPath);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelBackupPath);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(checkBoxUseTimeWindow);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(textBoxBackupInterval);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelBackupInterval);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(textBoxBackupEndTime);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelBackupEndTime);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(textBoxBackupStartTime);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelBackupStartTime);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(textBoxSchedule);
            collapsiblePanelSchedule.ContentPanel.Controls.Add(labelSchedule);
            // 
            // buttonBrowse
            // 
            buttonBrowse.Location = new Point(456, 168);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new Size(64, 24);
            buttonBrowse.TabIndex = 11;
            buttonBrowse.Text = "Browse...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += ButtonBrowse_Click;
            // 
            // textBoxBackupPath
            // 
            textBoxBackupPath.Location = new Point(120, 169);
            textBoxBackupPath.Name = "textBoxBackupPath";
            textBoxBackupPath.Size = new Size(330, 23);
            textBoxBackupPath.TabIndex = 10;
            // 
            // labelBackupPath
            // 
            labelBackupPath.AutoSize = true;
            labelBackupPath.Location = new Point(15, 172);
            labelBackupPath.Name = "labelBackupPath";
            labelBackupPath.Size = new Size(76, 15);
            labelBackupPath.TabIndex = 9;
            labelBackupPath.Text = "Backup Path:";
            // 
            // checkBoxUseTimeWindow
            // 
            checkBoxUseTimeWindow.AutoSize = true;
            checkBoxUseTimeWindow.Location = new Point(15, 40);
            checkBoxUseTimeWindow.Name = "checkBoxUseTimeWindow";
            checkBoxUseTimeWindow.Size = new Size(198, 19);
            checkBoxUseTimeWindow.TabIndex = 2;
            checkBoxUseTimeWindow.Text = "Enable Multiple Backups Per Day";
            checkBoxUseTimeWindow.UseVisualStyleBackColor = true;
            checkBoxUseTimeWindow.CheckedChanged += CheckBoxUseTimeWindow_CheckedChanged;
            // 
            // textBoxBackupInterval
            // 
            textBoxBackupInterval.Enabled = false;
            textBoxBackupInterval.Location = new Point(120, 134);
            textBoxBackupInterval.Name = "textBoxBackupInterval";
            textBoxBackupInterval.PlaceholderText = "e.g., 04:00:00";
            textBoxBackupInterval.Size = new Size(400, 23);
            textBoxBackupInterval.TabIndex = 8;
            // 
            // labelBackupInterval
            // 
            labelBackupInterval.AutoSize = true;
            labelBackupInterval.Enabled = false;
            labelBackupInterval.Location = new Point(30, 137);
            labelBackupInterval.Name = "labelBackupInterval";
            labelBackupInterval.Size = new Size(49, 15);
            labelBackupInterval.TabIndex = 7;
            labelBackupInterval.Text = "Interval:";
            // 
            // textBoxBackupEndTime
            // 
            textBoxBackupEndTime.Enabled = false;
            textBoxBackupEndTime.Location = new Point(120, 102);
            textBoxBackupEndTime.Name = "textBoxBackupEndTime";
            textBoxBackupEndTime.PlaceholderText = "e.g., 18:00:00";
            textBoxBackupEndTime.Size = new Size(400, 23);
            textBoxBackupEndTime.TabIndex = 6;
            // 
            // labelBackupEndTime
            // 
            labelBackupEndTime.AutoSize = true;
            labelBackupEndTime.Enabled = false;
            labelBackupEndTime.Location = new Point(30, 105);
            labelBackupEndTime.Name = "labelBackupEndTime";
            labelBackupEndTime.Size = new Size(60, 15);
            labelBackupEndTime.TabIndex = 5;
            labelBackupEndTime.Text = "End Time:";
            // 
            // textBoxBackupStartTime
            // 
            textBoxBackupStartTime.Enabled = false;
            textBoxBackupStartTime.Location = new Point(120, 70);
            textBoxBackupStartTime.Name = "textBoxBackupStartTime";
            textBoxBackupStartTime.PlaceholderText = "e.g., 08:00:00";
            textBoxBackupStartTime.Size = new Size(400, 23);
            textBoxBackupStartTime.TabIndex = 4;
            // 
            // labelBackupStartTime
            // 
            labelBackupStartTime.AutoSize = true;
            labelBackupStartTime.Enabled = false;
            labelBackupStartTime.Location = new Point(30, 73);
            labelBackupStartTime.Name = "labelBackupStartTime";
            labelBackupStartTime.Size = new Size(64, 15);
            labelBackupStartTime.TabIndex = 3;
            labelBackupStartTime.Text = "Start Time:";
            // 
            // textBoxSchedule
            // 
            textBoxSchedule.Location = new Point(120, 6);
            textBoxSchedule.Name = "textBoxSchedule";
            textBoxSchedule.PlaceholderText = "e.g., 02:00:00";
            textBoxSchedule.Size = new Size(400, 23);
            textBoxSchedule.TabIndex = 1;
            // 
            // labelSchedule
            // 
            labelSchedule.AutoSize = true;
            labelSchedule.Location = new Point(15, 9);
            labelSchedule.Name = "labelSchedule";
            labelSchedule.Size = new Size(100, 15);
            labelSchedule.TabIndex = 0;
            labelSchedule.Text = "Backup Schedule:";
            // 
            // numericUpDownRetentionDays
            // 
            numericUpDownRetentionDays.Location = new Point(120, 201);
            numericUpDownRetentionDays.Maximum = new decimal(new int[] { 3650, 0, 0, 0 });
            numericUpDownRetentionDays.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownRetentionDays.Name = "numericUpDownRetentionDays";
            numericUpDownRetentionDays.Size = new Size(100, 23);
            numericUpDownRetentionDays.TabIndex = 12;
            numericUpDownRetentionDays.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // labelRetentionDays
            // 
            labelRetentionDays.AutoSize = true;
            labelRetentionDays.Location = new Point(15, 203);
            labelRetentionDays.Name = "labelRetentionDays";
            labelRetentionDays.Size = new Size(91, 15);
            labelRetentionDays.TabIndex = 13;
            labelRetentionDays.Text = "Retention Days:";
            // 
            // numericUpDownMinBackupFiles
            // 
            numericUpDownMinBackupFiles.Location = new Point(120, 233);
            numericUpDownMinBackupFiles.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numericUpDownMinBackupFiles.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownMinBackupFiles.Name = "numericUpDownMinBackupFiles";
            numericUpDownMinBackupFiles.Size = new Size(100, 23);
            numericUpDownMinBackupFiles.TabIndex = 14;
            numericUpDownMinBackupFiles.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // labelMinBackupFiles
            // 
            labelMinBackupFiles.AutoSize = true;
            labelMinBackupFiles.Location = new Point(15, 235);
            labelMinBackupFiles.Name = "labelMinBackupFiles";
            labelMinBackupFiles.Size = new Size(98, 15);
            labelMinBackupFiles.TabIndex = 15;
            labelMinBackupFiles.Text = "Min Backup Files:";
            // 
            // collapsiblePanelAzure
            // 
            collapsiblePanelAzure.HeaderText = "Azure Blob Storage (Optional)";
            collapsiblePanelAzure.IsExpanded = false;
            collapsiblePanelAzure.ExpandedHeight = 240;
            collapsiblePanelAzure.Location = new Point(12, 562);
            collapsiblePanelAzure.Name = "collapsiblePanelAzure";
            collapsiblePanelAzure.Size = new Size(560, 40);
            collapsiblePanelAzure.TabIndex = 2;
            collapsiblePanelAzure.ExpandedChanged += (s, e) => RepositionPanels();
            collapsiblePanelAzure.ContentPanel.Controls.Add(buttonTestAzure);
            collapsiblePanelAzure.ContentPanel.Controls.Add(textBoxAzureBlobPrefix);
            collapsiblePanelAzure.ContentPanel.Controls.Add(labelAzureBlobPrefix);
            collapsiblePanelAzure.ContentPanel.Controls.Add(textBoxAzureContainerName);
            collapsiblePanelAzure.ContentPanel.Controls.Add(labelAzureContainerName);
            collapsiblePanelAzure.ContentPanel.Controls.Add(textBoxAzureSasToken);
            collapsiblePanelAzure.ContentPanel.Controls.Add(labelAzureSasToken);
            collapsiblePanelAzure.ContentPanel.Controls.Add(textBoxAzureStorageAccount);
            collapsiblePanelAzure.ContentPanel.Controls.Add(labelAzureStorageAccount);
            collapsiblePanelAzure.ContentPanel.Controls.Add(textBoxAzureConnectionString);
            collapsiblePanelAzure.ContentPanel.Controls.Add(labelAzureConnectionString);
            collapsiblePanelAzure.ContentPanel.Controls.Add(radioButtonSasToken);
            collapsiblePanelAzure.ContentPanel.Controls.Add(radioButtonConnectionString);
            collapsiblePanelAzure.ContentPanel.Controls.Add(checkBoxEnableAzure);
            // 
            // buttonTestAzure
            // 
            buttonTestAzure.Enabled = false;
            buttonTestAzure.Location = new Point(456, 153);
            buttonTestAzure.Name = "buttonTestAzure";
            buttonTestAzure.Size = new Size(64, 24);
            buttonTestAzure.TabIndex = 11;
            buttonTestAzure.Text = "Test";
            buttonTestAzure.UseVisualStyleBackColor = true;
            buttonTestAzure.Click += ButtonTestAzure_Click;
            // 
            // textBoxAzureBlobPrefix
            // 
            textBoxAzureBlobPrefix.Enabled = false;
            textBoxAzureBlobPrefix.Location = new Point(120, 186);
            textBoxAzureBlobPrefix.Name = "textBoxAzureBlobPrefix";
            textBoxAzureBlobPrefix.PlaceholderText = "e.g., client-name/ or folder/";
            textBoxAzureBlobPrefix.Size = new Size(400, 23);
            textBoxAzureBlobPrefix.TabIndex = 13;
            // 
            // labelAzureBlobPrefix
            // 
            labelAzureBlobPrefix.AutoSize = true;
            labelAzureBlobPrefix.Location = new Point(15, 189);
            labelAzureBlobPrefix.Name = "labelAzureBlobPrefix";
            labelAzureBlobPrefix.Size = new Size(66, 15);
            labelAzureBlobPrefix.TabIndex = 12;
            labelAzureBlobPrefix.Text = "Blob Prefix:";
            // 
            // textBoxAzureContainerName
            // 
            textBoxAzureContainerName.Enabled = false;
            textBoxAzureContainerName.Location = new Point(120, 154);
            textBoxAzureContainerName.Name = "textBoxAzureContainerName";
            textBoxAzureContainerName.PlaceholderText = "e.g., database-backups";
            textBoxAzureContainerName.Size = new Size(330, 23);
            textBoxAzureContainerName.TabIndex = 10;
            // 
            // labelAzureContainerName
            // 
            labelAzureContainerName.AutoSize = true;
            labelAzureContainerName.Location = new Point(15, 157);
            labelAzureContainerName.Name = "labelAzureContainerName";
            labelAzureContainerName.Size = new Size(97, 15);
            labelAzureContainerName.TabIndex = 9;
            labelAzureContainerName.Text = "Container Name:";
            // 
            // textBoxAzureSasToken
            // 
            textBoxAzureSasToken.Enabled = false;
            textBoxAzureSasToken.Location = new Point(120, 122);
            textBoxAzureSasToken.Name = "textBoxAzureSasToken";
            textBoxAzureSasToken.PlaceholderText = "e.g., ?sv=2021-06-08&ss=b&srt=sco&sp=...";
            textBoxAzureSasToken.Size = new Size(400, 23);
            textBoxAzureSasToken.TabIndex = 8;
            textBoxAzureSasToken.UseSystemPasswordChar = true;
            textBoxAzureSasToken.Visible = false;
            // 
            // labelAzureSasToken
            // 
            labelAzureSasToken.AutoSize = true;
            labelAzureSasToken.Location = new Point(15, 125);
            labelAzureSasToken.Name = "labelAzureSasToken";
            labelAzureSasToken.Size = new Size(65, 15);
            labelAzureSasToken.TabIndex = 7;
            labelAzureSasToken.Text = "SAS Token:";
            labelAzureSasToken.Visible = false;
            // 
            // textBoxAzureStorageAccount
            // 
            textBoxAzureStorageAccount.Enabled = false;
            textBoxAzureStorageAccount.Location = new Point(120, 90);
            textBoxAzureStorageAccount.Name = "textBoxAzureStorageAccount";
            textBoxAzureStorageAccount.PlaceholderText = "e.g., mystorageaccount";
            textBoxAzureStorageAccount.Size = new Size(400, 23);
            textBoxAzureStorageAccount.TabIndex = 6;
            textBoxAzureStorageAccount.Visible = false;
            // 
            // labelAzureStorageAccount
            // 
            labelAzureStorageAccount.AutoSize = true;
            labelAzureStorageAccount.Location = new Point(15, 93);
            labelAzureStorageAccount.Name = "labelAzureStorageAccount";
            labelAzureStorageAccount.Size = new Size(98, 15);
            labelAzureStorageAccount.TabIndex = 5;
            labelAzureStorageAccount.Text = "Storage Account:";
            labelAzureStorageAccount.Visible = false;
            // 
            // textBoxAzureConnectionString
            // 
            textBoxAzureConnectionString.Enabled = false;
            textBoxAzureConnectionString.Location = new Point(120, 58);
            textBoxAzureConnectionString.Name = "textBoxAzureConnectionString";
            textBoxAzureConnectionString.Size = new Size(400, 23);
            textBoxAzureConnectionString.TabIndex = 4;
            textBoxAzureConnectionString.UseSystemPasswordChar = true;
            // 
            // labelAzureConnectionString
            // 
            labelAzureConnectionString.AutoSize = true;
            labelAzureConnectionString.Location = new Point(15, 61);
            labelAzureConnectionString.Name = "labelAzureConnectionString";
            labelAzureConnectionString.Size = new Size(106, 15);
            labelAzureConnectionString.TabIndex = 3;
            labelAzureConnectionString.Text = "Connection String:";
            // 
            // radioButtonSasToken
            // 
            radioButtonSasToken.AutoSize = true;
            radioButtonSasToken.Enabled = false;
            radioButtonSasToken.Location = new Point(150, 30);
            radioButtonSasToken.Name = "radioButtonSasToken";
            radioButtonSasToken.Size = new Size(172, 19);
            radioButtonSasToken.TabIndex = 2;
            radioButtonSasToken.Text = "SAS Token (Recommended)";
            radioButtonSasToken.UseVisualStyleBackColor = true;
            radioButtonSasToken.CheckedChanged += RadioButtonAuthMethod_CheckedChanged;
            // 
            // radioButtonConnectionString
            // 
            radioButtonConnectionString.AutoSize = true;
            radioButtonConnectionString.Checked = true;
            radioButtonConnectionString.Enabled = false;
            radioButtonConnectionString.Location = new Point(15, 30);
            radioButtonConnectionString.Name = "radioButtonConnectionString";
            radioButtonConnectionString.Size = new Size(121, 19);
            radioButtonConnectionString.TabIndex = 1;
            radioButtonConnectionString.TabStop = true;
            radioButtonConnectionString.Text = "Connection String";
            radioButtonConnectionString.UseVisualStyleBackColor = true;
            radioButtonConnectionString.CheckedChanged += RadioButtonAuthMethod_CheckedChanged;
            // 
            // checkBoxEnableAzure
            // 
            checkBoxEnableAzure.AutoSize = true;
            checkBoxEnableAzure.Location = new Point(15, 5);
            checkBoxEnableAzure.Name = "checkBoxEnableAzure";
            checkBoxEnableAzure.Size = new Size(206, 19);
            checkBoxEnableAzure.TabIndex = 0;
            checkBoxEnableAzure.Text = "Enable Azure Blob Storage Backup";
            checkBoxEnableAzure.UseVisualStyleBackColor = true;
            checkBoxEnableAzure.CheckedChanged += CheckBoxEnableAzure_CheckedChanged;
            // 
            // collapsiblePanelEmail
            // 
            collapsiblePanelEmail.HeaderText = "Email Notifications (Optional)";
            collapsiblePanelEmail.IsExpanded = false;
            collapsiblePanelEmail.ExpandedHeight = 155;
            collapsiblePanelEmail.Location = new Point(12, 612);
            collapsiblePanelEmail.Name = "collapsiblePanelEmail";
            collapsiblePanelEmail.Size = new Size(560, 40);
            collapsiblePanelEmail.TabIndex = 3;
            collapsiblePanelEmail.ExpandedChanged += (s, e) => RepositionPanels();
            collapsiblePanelEmail.ContentPanel.Controls.Add(linkLabelGmailSetup);
            collapsiblePanelEmail.ContentPanel.Controls.Add(labelEmailInfo);
            collapsiblePanelEmail.ContentPanel.Controls.Add(textBoxEmailRecipient);
            collapsiblePanelEmail.ContentPanel.Controls.Add(labelEmailRecipient);
            collapsiblePanelEmail.ContentPanel.Controls.Add(textBoxEmailPassword);
            collapsiblePanelEmail.ContentPanel.Controls.Add(labelEmailPassword);
            collapsiblePanelEmail.ContentPanel.Controls.Add(textBoxEmailSender);
            collapsiblePanelEmail.ContentPanel.Controls.Add(labelEmailSender);
            // 
            // linkLabelGmailSetup
            // 
            linkLabelGmailSetup.AutoSize = true;
            linkLabelGmailSetup.Location = new Point(300, 98);
            linkLabelGmailSetup.Name = "linkLabelGmailSetup";
            linkLabelGmailSetup.Size = new Size(144, 15);
            linkLabelGmailSetup.TabIndex = 7;
            linkLabelGmailSetup.TabStop = true;
            linkLabelGmailSetup.Text = "How to get App Password";
            linkLabelGmailSetup.LinkClicked += LinkLabelGmailSetup_LinkClicked;
            // 
            // labelEmailInfo
            // 
            labelEmailInfo.AutoSize = true;
            labelEmailInfo.ForeColor = SystemColors.GrayText;
            labelEmailInfo.Location = new Point(15, 98);
            labelEmailInfo.Name = "labelEmailInfo";
            labelEmailInfo.Size = new Size(239, 15);
            labelEmailInfo.TabIndex = 6;
            labelEmailInfo.Text = "Get email alerts when database backups fail.";
            // 
            // textBoxEmailRecipient
            // 
            textBoxEmailRecipient.Location = new Point(120, 69);
            textBoxEmailRecipient.Name = "textBoxEmailRecipient";
            textBoxEmailRecipient.PlaceholderText = "admin@yourcompany.com";
            textBoxEmailRecipient.Size = new Size(400, 23);
            textBoxEmailRecipient.TabIndex = 5;
            // 
            // labelEmailRecipient
            // 
            labelEmailRecipient.AutoSize = true;
            labelEmailRecipient.Location = new Point(15, 72);
            labelEmailRecipient.Name = "labelEmailRecipient";
            labelEmailRecipient.Size = new Size(85, 15);
            labelEmailRecipient.TabIndex = 4;
            labelEmailRecipient.Text = "Send Alerts To:";
            // 
            // textBoxEmailPassword
            // 
            textBoxEmailPassword.Location = new Point(120, 37);
            textBoxEmailPassword.Name = "textBoxEmailPassword";
            textBoxEmailPassword.PlaceholderText = "16-character app password";
            textBoxEmailPassword.Size = new Size(400, 23);
            textBoxEmailPassword.TabIndex = 3;
            textBoxEmailPassword.UseSystemPasswordChar = true;
            // 
            // labelEmailPassword
            // 
            labelEmailPassword.AutoSize = true;
            labelEmailPassword.Location = new Point(15, 40);
            labelEmailPassword.Name = "labelEmailPassword";
            labelEmailPassword.Size = new Size(85, 15);
            labelEmailPassword.TabIndex = 2;
            labelEmailPassword.Text = "App Password:";
            // 
            // textBoxEmailSender
            // 
            textBoxEmailSender.Location = new Point(120, 5);
            textBoxEmailSender.Name = "textBoxEmailSender";
            textBoxEmailSender.PlaceholderText = "yourname@gmail.com";
            textBoxEmailSender.Size = new Size(400, 23);
            textBoxEmailSender.TabIndex = 1;
            // 
            // labelEmailSender
            // 
            labelEmailSender.AutoSize = true;
            labelEmailSender.Location = new Point(15, 8);
            labelEmailSender.Name = "labelEmailSender";
            labelEmailSender.Size = new Size(86, 15);
            labelEmailSender.TabIndex = 0;
            labelEmailSender.Text = "Gmail Address:";
            // 
            // buttonSave
            // 
            buttonSave.BackColor = Color.FromArgb(0, 120, 215);
            buttonSave.FlatStyle = FlatStyle.Flat;
            buttonSave.ForeColor = Color.White;
            buttonSave.Location = new Point(467, 668);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(105, 35);
            buttonSave.TabIndex = 4;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = false;
            buttonSave.Click += ButtonSave_Click;
            // 
            // buttonTestConnection
            // 
            buttonTestConnection.FlatStyle = FlatStyle.Flat;
            buttonTestConnection.Location = new Point(332, 668);
            buttonTestConnection.Name = "buttonTestConnection";
            buttonTestConnection.Size = new Size(120, 35);
            buttonTestConnection.TabIndex = 5;
            buttonTestConnection.Text = "Test Connection";
            buttonTestConnection.UseVisualStyleBackColor = true;
            buttonTestConnection.Click += ButtonTestConnection_Click;
            // 
            // panelMain
            // 
            panelMain.AutoScroll = true;
            panelMain.Location = new Point(0, 0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(584, 660);
            panelMain.TabIndex = 6;
            panelMain.Controls.Add(collapsiblePanelDatabase);
            panelMain.Controls.Add(collapsiblePanelSchedule);
            panelMain.Controls.Add(collapsiblePanelAzure);
            panelMain.Controls.Add(collapsiblePanelEmail);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(250, 250, 250);
            ClientSize = new Size(584, 715);
            Controls.Add(panelMain);
            Controls.Add(buttonTestConnection);
            Controls.Add(buttonSave);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Database Backup Service Configuration";
            Icon = ((Icon?)resources.GetObject("$this.Icon"));
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)numericUpDownRetentionDays).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMinBackupFiles).EndInit();
            panelMain.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private CollapsiblePanel collapsiblePanelDatabase;
        private System.Windows.Forms.ComboBox comboBoxDatabaseType;
        private System.Windows.Forms.Label labelDatabaseType;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox textBoxServerName;
        private System.Windows.Forms.Label labelServerName;
        private System.Windows.Forms.TextBox textBoxDatabaseName;
        private System.Windows.Forms.Label labelDatabaseName;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private CollapsiblePanel collapsiblePanelSchedule;
        private System.Windows.Forms.TextBox textBoxSchedule;
        private System.Windows.Forms.Label labelSchedule;
        private System.Windows.Forms.CheckBox checkBoxUseTimeWindow;
        private System.Windows.Forms.TextBox textBoxBackupStartTime;
        private System.Windows.Forms.Label labelBackupStartTime;
        private System.Windows.Forms.TextBox textBoxBackupEndTime;
        private System.Windows.Forms.Label labelBackupEndTime;
        private System.Windows.Forms.TextBox textBoxBackupInterval;
        private System.Windows.Forms.Label labelBackupInterval;
        private System.Windows.Forms.TextBox textBoxBackupPath;
        private System.Windows.Forms.Label labelBackupPath;
        private System.Windows.Forms.Button buttonBrowse;
        private CollapsiblePanel collapsiblePanelAzure;
        private System.Windows.Forms.CheckBox checkBoxEnableAzure;
        private System.Windows.Forms.RadioButton radioButtonConnectionString;
        private System.Windows.Forms.RadioButton radioButtonSasToken;
        private System.Windows.Forms.TextBox textBoxAzureConnectionString;
        private System.Windows.Forms.Label labelAzureConnectionString;
        private System.Windows.Forms.TextBox textBoxAzureStorageAccount;
        private System.Windows.Forms.Label labelAzureStorageAccount;
        private System.Windows.Forms.TextBox textBoxAzureSasToken;
        private System.Windows.Forms.Label labelAzureSasToken;
        private System.Windows.Forms.TextBox textBoxAzureContainerName;
        private System.Windows.Forms.Label labelAzureContainerName;
        private System.Windows.Forms.TextBox textBoxAzureBlobPrefix;
        private System.Windows.Forms.Label labelAzureBlobPrefix;
        private System.Windows.Forms.Button buttonTestAzure;
        private CollapsiblePanel collapsiblePanelEmail;
        private System.Windows.Forms.TextBox textBoxEmailSender;
        private System.Windows.Forms.Label labelEmailSender;
        private System.Windows.Forms.TextBox textBoxEmailPassword;
        private System.Windows.Forms.Label labelEmailPassword;
        private System.Windows.Forms.TextBox textBoxEmailRecipient;
        private System.Windows.Forms.Label labelEmailRecipient;
        private System.Windows.Forms.Label labelEmailInfo;
        private System.Windows.Forms.LinkLabel linkLabelGmailSetup;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonTestConnection;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.NumericUpDown numericUpDownRetentionDays;
        private System.Windows.Forms.Label labelRetentionDays;
        private System.Windows.Forms.NumericUpDown numericUpDownMinBackupFiles;
        private System.Windows.Forms.Label labelMinBackupFiles;
        private System.Windows.Forms.Panel panelMain;
    }
}

