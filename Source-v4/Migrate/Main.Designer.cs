namespace Migrate
{
	partial class Main
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.txtOrganizationServiceUrl = new System.Windows.Forms.TextBox();
			this.rdoADAuth = new System.Windows.Forms.RadioButton();
			this.rdoIFDAuth = new System.Windows.Forms.RadioButton();
			this.authGroup = new System.Windows.Forms.GroupBox();
			this.txtPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtUsername = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			this.migrateGroup = new System.Windows.Forms.GroupBox();
			this.lblProgress = new System.Windows.Forms.Label();
			this.lblMessage = new System.Windows.Forms.Label();
			this.pgbProgress = new System.Windows.Forms.ProgressBar();
			this.label6 = new System.Windows.Forms.Label();
			this.directionGroup = new System.Windows.Forms.GroupBox();
			this.chkMoveAnnotations = new System.Windows.Forms.CheckBox();
			this.chkMigrateAttachments = new System.Windows.Forms.CheckBox();
			this.chkMigrateAnnotations = new System.Windows.Forms.CheckBox();
			this.rdoOutbound = new System.Windows.Forms.RadioButton();
			this.rdoInbound = new System.Windows.Forms.RadioButton();
			this.btnMigrate = new System.Windows.Forms.Button();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblCompression = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.txtAES256Salt = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.txtAES256Key = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.txtAzureAccountKey = new System.Windows.Forms.TextBox();
			this.txtAzureAccount = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.chkPluginStepsManage = new System.Windows.Forms.CheckBox();
			this.udWaitDelay = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.udThreadCount = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.lblEncryption = new System.Windows.Forms.Label();
			this.authGroup.SuspendLayout();
			this.migrateGroup.SuspendLayout();
			this.directionGroup.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udWaitDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udThreadCount)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(121, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "OrganizationServiceUrl :";
			// 
			// txtOrganizationServiceUrl
			// 
			this.txtOrganizationServiceUrl.Location = new System.Drawing.Point(139, 9);
			this.txtOrganizationServiceUrl.Name = "txtOrganizationServiceUrl";
			this.txtOrganizationServiceUrl.Size = new System.Drawing.Size(648, 20);
			this.txtOrganizationServiceUrl.TabIndex = 1;
			this.txtOrganizationServiceUrl.Text = "https://{server}/{org}/XRMServices/2011/Organization.svc";
			// 
			// rdoADAuth
			// 
			this.rdoADAuth.AutoSize = true;
			this.rdoADAuth.Checked = true;
			this.rdoADAuth.Location = new System.Drawing.Point(16, 19);
			this.rdoADAuth.Name = "rdoADAuth";
			this.rdoADAuth.Size = new System.Drawing.Size(111, 17);
			this.rdoADAuth.TabIndex = 2;
			this.rdoADAuth.TabStop = true;
			this.rdoADAuth.Text = "AD Authentication";
			this.rdoADAuth.UseVisualStyleBackColor = true;
			// 
			// rdoIFDAuth
			// 
			this.rdoIFDAuth.AutoSize = true;
			this.rdoIFDAuth.Location = new System.Drawing.Point(151, 19);
			this.rdoIFDAuth.Name = "rdoIFDAuth";
			this.rdoIFDAuth.Size = new System.Drawing.Size(148, 17);
			this.rdoIFDAuth.TabIndex = 3;
			this.rdoIFDAuth.Text = "IFD/Online Authentication";
			this.rdoIFDAuth.UseVisualStyleBackColor = true;
			this.rdoIFDAuth.CheckedChanged += new System.EventHandler(this.rdoIFDAuth_CheckedChanged);
			// 
			// authGroup
			// 
			this.authGroup.Controls.Add(this.txtPassword);
			this.authGroup.Controls.Add(this.label3);
			this.authGroup.Controls.Add(this.txtUsername);
			this.authGroup.Controls.Add(this.label2);
			this.authGroup.Controls.Add(this.btnConnect);
			this.authGroup.Controls.Add(this.rdoADAuth);
			this.authGroup.Controls.Add(this.rdoIFDAuth);
			this.authGroup.Location = new System.Drawing.Point(15, 35);
			this.authGroup.Name = "authGroup";
			this.authGroup.Size = new System.Drawing.Size(772, 110);
			this.authGroup.TabIndex = 4;
			this.authGroup.TabStop = false;
			this.authGroup.Text = "Authentication";
			// 
			// txtPassword
			// 
			this.txtPassword.Enabled = false;
			this.txtPassword.Location = new System.Drawing.Point(221, 72);
			this.txtPassword.Name = "txtPassword";
			this.txtPassword.Size = new System.Drawing.Size(221, 20);
			this.txtPassword.TabIndex = 9;
			this.txtPassword.UseSystemPasswordChar = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(154, 75);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Password :";
			// 
			// txtUsername
			// 
			this.txtUsername.Enabled = false;
			this.txtUsername.Location = new System.Drawing.Point(221, 46);
			this.txtUsername.Name = "txtUsername";
			this.txtUsername.Size = new System.Drawing.Size(221, 20);
			this.txtUsername.TabIndex = 8;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(154, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(61, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Username :";
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(565, 14);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(93, 27);
			this.btnConnect.TabIndex = 7;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// migrateGroup
			// 
			this.migrateGroup.Controls.Add(this.groupBox2);
			this.migrateGroup.Controls.Add(this.groupBox1);
			this.migrateGroup.Controls.Add(this.lblProgress);
			this.migrateGroup.Controls.Add(this.lblMessage);
			this.migrateGroup.Controls.Add(this.pgbProgress);
			this.migrateGroup.Controls.Add(this.label6);
			this.migrateGroup.Controls.Add(this.directionGroup);
			this.migrateGroup.Controls.Add(this.btnMigrate);
			this.migrateGroup.Controls.Add(this.txtOutput);
			this.migrateGroup.Enabled = false;
			this.migrateGroup.Location = new System.Drawing.Point(15, 151);
			this.migrateGroup.Name = "migrateGroup";
			this.migrateGroup.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.migrateGroup.Size = new System.Drawing.Size(772, 550);
			this.migrateGroup.TabIndex = 7;
			this.migrateGroup.TabStop = false;
			// 
			// lblProgress
			// 
			this.lblProgress.Location = new System.Drawing.Point(710, 244);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new System.Drawing.Size(49, 23);
			this.lblProgress.TabIndex = 18;
			this.lblProgress.Text = "0%";
			this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblMessage
			// 
			this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblMessage.Location = new System.Drawing.Point(9, 270);
			this.lblMessage.Margin = new System.Windows.Forms.Padding(0);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(750, 38);
			this.lblMessage.TabIndex = 17;
			this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pgbProgress
			// 
			this.pgbProgress.Location = new System.Drawing.Point(9, 244);
			this.pgbProgress.Name = "pgbProgress";
			this.pgbProgress.Size = new System.Drawing.Size(695, 23);
			this.pgbProgress.Step = 1;
			this.pgbProgress.TabIndex = 16;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(6, 321);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 13);
			this.label6.TabIndex = 15;
			this.label6.Text = "Errors :";
			// 
			// directionGroup
			// 
			this.directionGroup.Controls.Add(this.chkMoveAnnotations);
			this.directionGroup.Controls.Add(this.chkMigrateAttachments);
			this.directionGroup.Controls.Add(this.chkMigrateAnnotations);
			this.directionGroup.Controls.Add(this.rdoOutbound);
			this.directionGroup.Controls.Add(this.rdoInbound);
			this.directionGroup.Location = new System.Drawing.Point(9, 147);
			this.directionGroup.Name = "directionGroup";
			this.directionGroup.Size = new System.Drawing.Size(328, 91);
			this.directionGroup.TabIndex = 7;
			this.directionGroup.TabStop = false;
			this.directionGroup.Text = "Migration";
			// 
			// chkMoveAnnotations
			// 
			this.chkMoveAnnotations.AutoSize = true;
			this.chkMoveAnnotations.Location = new System.Drawing.Point(16, 68);
			this.chkMoveAnnotations.Name = "chkMoveAnnotations";
			this.chkMoveAnnotations.Size = new System.Drawing.Size(153, 17);
			this.chkMoveAnnotations.TabIndex = 24;
			this.chkMoveAnnotations.Text = "Move Annotations External";
			this.chkMoveAnnotations.UseVisualStyleBackColor = true;
			this.chkMoveAnnotations.CheckedChanged += new System.EventHandler(this.chkMoveAnnotations_CheckedChanged);
			// 
			// chkMigrateAttachments
			// 
			this.chkMigrateAttachments.AutoSize = true;
			this.chkMigrateAttachments.Checked = true;
			this.chkMigrateAttachments.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkMigrateAttachments.Location = new System.Drawing.Point(150, 48);
			this.chkMigrateAttachments.Name = "chkMigrateAttachments";
			this.chkMigrateAttachments.Size = new System.Drawing.Size(151, 17);
			this.chkMigrateAttachments.TabIndex = 16;
			this.chkMigrateAttachments.Text = "Migrate Email Attachments";
			this.chkMigrateAttachments.UseVisualStyleBackColor = true;
			// 
			// chkMigrateAnnotations
			// 
			this.chkMigrateAnnotations.AutoSize = true;
			this.chkMigrateAnnotations.Checked = true;
			this.chkMigrateAnnotations.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkMigrateAnnotations.Location = new System.Drawing.Point(16, 48);
			this.chkMigrateAnnotations.Name = "chkMigrateAnnotations";
			this.chkMigrateAnnotations.Size = new System.Drawing.Size(120, 17);
			this.chkMigrateAnnotations.TabIndex = 15;
			this.chkMigrateAnnotations.Text = "Migrate Annotations";
			this.chkMigrateAnnotations.UseVisualStyleBackColor = true;
			// 
			// rdoOutbound
			// 
			this.rdoOutbound.AutoSize = true;
			this.rdoOutbound.Checked = true;
			this.rdoOutbound.Location = new System.Drawing.Point(16, 19);
			this.rdoOutbound.Name = "rdoOutbound";
			this.rdoOutbound.Size = new System.Drawing.Size(102, 17);
			this.rdoOutbound.TabIndex = 2;
			this.rdoOutbound.TabStop = true;
			this.rdoOutbound.Text = "CRM -> External";
			this.rdoOutbound.UseVisualStyleBackColor = true;
			// 
			// rdoInbound
			// 
			this.rdoInbound.AutoSize = true;
			this.rdoInbound.Location = new System.Drawing.Point(151, 19);
			this.rdoInbound.Name = "rdoInbound";
			this.rdoInbound.Size = new System.Drawing.Size(102, 17);
			this.rdoInbound.TabIndex = 3;
			this.rdoInbound.Text = "External -> CRM";
			this.rdoInbound.UseVisualStyleBackColor = true;
			this.rdoInbound.CheckedChanged += new System.EventHandler(this.rdoInbound_CheckedChanged);
			// 
			// btnMigrate
			// 
			this.btnMigrate.Location = new System.Drawing.Point(604, 482);
			this.btnMigrate.Name = "btnMigrate";
			this.btnMigrate.Size = new System.Drawing.Size(155, 54);
			this.btnMigrate.TabIndex = 9;
			this.btnMigrate.Text = "Migrate";
			this.btnMigrate.UseVisualStyleBackColor = true;
			this.btnMigrate.Click += new System.EventHandler(this.btnMigrate_Click);
			// 
			// txtOutput
			// 
			this.txtOutput.Location = new System.Drawing.Point(9, 337);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.ReadOnly = true;
			this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtOutput.Size = new System.Drawing.Size(750, 139);
			this.txtOutput.TabIndex = 8;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lblEncryption);
			this.groupBox1.Controls.Add(this.lblCompression);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.txtAES256Salt);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.txtAES256Key);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.txtAzureAccountKey);
			this.groupBox1.Controls.Add(this.txtAzureAccount);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Location = new System.Drawing.Point(9, 19);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(750, 122);
			this.groupBox1.TabIndex = 19;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Settings";
			// 
			// lblCompression
			// 
			this.lblCompression.AutoSize = true;
			this.lblCompression.Location = new System.Drawing.Point(504, 18);
			this.lblCompression.Name = "lblCompression";
			this.lblCompression.Size = new System.Drawing.Size(73, 13);
			this.lblCompression.TabIndex = 38;
			this.lblCompression.Text = "Compression :";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(6, 97);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(73, 13);
			this.label10.TabIndex = 37;
			this.label10.Text = "AES256 Salt :";
			// 
			// txtAES256Salt
			// 
			this.txtAES256Salt.Enabled = false;
			this.txtAES256Salt.Location = new System.Drawing.Point(110, 94);
			this.txtAES256Salt.Name = "txtAES256Salt";
			this.txtAES256Salt.Size = new System.Drawing.Size(628, 20);
			this.txtAES256Salt.TabIndex = 36;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(6, 71);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(73, 13);
			this.label9.TabIndex = 35;
			this.label9.Text = "AES256 Key :";
			// 
			// txtAES256Key
			// 
			this.txtAES256Key.Enabled = false;
			this.txtAES256Key.Location = new System.Drawing.Point(110, 68);
			this.txtAES256Key.Name = "txtAES256Key";
			this.txtAES256Key.Size = new System.Drawing.Size(628, 20);
			this.txtAES256Key.TabIndex = 34;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(6, 45);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(104, 13);
			this.label8.TabIndex = 33;
			this.label8.Text = "Azure Account Key :";
			// 
			// txtAzureAccountKey
			// 
			this.txtAzureAccountKey.Enabled = false;
			this.txtAzureAccountKey.Location = new System.Drawing.Point(110, 42);
			this.txtAzureAccountKey.Name = "txtAzureAccountKey";
			this.txtAzureAccountKey.Size = new System.Drawing.Size(628, 20);
			this.txtAzureAccountKey.TabIndex = 32;
			// 
			// txtAzureAccount
			// 
			this.txtAzureAccount.Enabled = false;
			this.txtAzureAccount.Location = new System.Drawing.Point(110, 15);
			this.txtAzureAccount.Name = "txtAzureAccount";
			this.txtAzureAccount.Size = new System.Drawing.Size(204, 20);
			this.txtAzureAccount.TabIndex = 30;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(6, 18);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(83, 13);
			this.label7.TabIndex = 31;
			this.label7.Text = "Azure Account :";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.chkPluginStepsManage);
			this.groupBox2.Controls.Add(this.udWaitDelay);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.udThreadCount);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Location = new System.Drawing.Point(354, 147);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(405, 91);
			this.groupBox2.TabIndex = 20;
			this.groupBox2.TabStop = false;
			// 
			// chkPluginStepsManage
			// 
			this.chkPluginStepsManage.AutoSize = true;
			this.chkPluginStepsManage.Checked = true;
			this.chkPluginStepsManage.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkPluginStepsManage.Location = new System.Drawing.Point(21, 20);
			this.chkPluginStepsManage.Name = "chkPluginStepsManage";
			this.chkPluginStepsManage.Size = new System.Drawing.Size(300, 17);
			this.chkPluginStepsManage.TabIndex = 19;
			this.chkPluginStepsManage.Text = "Automatically Enable/Disable BinaryStorageOptions plugin";
			this.chkPluginStepsManage.UseVisualStyleBackColor = true;
			// 
			// udWaitDelay
			// 
			this.udWaitDelay.Location = new System.Drawing.Point(244, 50);
			this.udWaitDelay.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
			this.udWaitDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.udWaitDelay.Name = "udWaitDelay";
			this.udWaitDelay.Size = new System.Drawing.Size(47, 20);
			this.udWaitDelay.TabIndex = 18;
			this.udWaitDelay.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(159, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(79, 13);
			this.label5.TabIndex = 17;
			this.label5.Text = "Wait Delay (s) :";
			// 
			// udThreadCount
			// 
			this.udThreadCount.Location = new System.Drawing.Point(103, 48);
			this.udThreadCount.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
			this.udThreadCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.udThreadCount.Name = "udThreadCount";
			this.udThreadCount.Size = new System.Drawing.Size(47, 20);
			this.udThreadCount.TabIndex = 16;
			this.udThreadCount.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(18, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 13);
			this.label4.TabIndex = 15;
			this.label4.Text = "ThreadCount :";
			// 
			// lblEncryption
			// 
			this.lblEncryption.AutoSize = true;
			this.lblEncryption.Location = new System.Drawing.Point(329, 18);
			this.lblEncryption.Name = "lblEncryption";
			this.lblEncryption.Size = new System.Drawing.Size(63, 13);
			this.lblEncryption.TabIndex = 39;
			this.lblEncryption.Text = "Encryption :";
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 712);
			this.Controls.Add(this.migrateGroup);
			this.Controls.Add(this.authGroup);
			this.Controls.Add(this.txtOrganizationServiceUrl);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Main";
			this.Text = "Crm Binary Storage Migration";
			this.Load += new System.EventHandler(this.Main_Load);
			this.authGroup.ResumeLayout(false);
			this.authGroup.PerformLayout();
			this.migrateGroup.ResumeLayout(false);
			this.migrateGroup.PerformLayout();
			this.directionGroup.ResumeLayout(false);
			this.directionGroup.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.udWaitDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udThreadCount)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtOrganizationServiceUrl;
		private System.Windows.Forms.RadioButton rdoADAuth;
		private System.Windows.Forms.RadioButton rdoIFDAuth;
		private System.Windows.Forms.GroupBox authGroup;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.GroupBox migrateGroup;
		private System.Windows.Forms.Button btnMigrate;
		private System.Windows.Forms.TextBox txtOutput;
		private System.Windows.Forms.GroupBox directionGroup;
		private System.Windows.Forms.RadioButton rdoOutbound;
		private System.Windows.Forms.RadioButton rdoInbound;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtUsername;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.ProgressBar pgbProgress;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckBox chkMigrateAttachments;
		private System.Windows.Forms.CheckBox chkMigrateAnnotations;
		private System.Windows.Forms.CheckBox chkMoveAnnotations;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblCompression;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txtAES256Salt;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtAES256Key;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtAzureAccountKey;
		private System.Windows.Forms.TextBox txtAzureAccount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox chkPluginStepsManage;
		private System.Windows.Forms.NumericUpDown udWaitDelay;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown udThreadCount;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblEncryption;
	}
}

