namespace ip4scanNtag
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.mnuRestart = new System.Windows.Forms.MenuItem();
            this.mnuDisconnectReader = new System.Windows.Forms.MenuItem();
            this.mnuConnectReader = new System.Windows.Forms.MenuItem();
            this.mnuExit = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mnuEnableLogging = new System.Windows.Forms.MenuItem();
            this.mnuWriteTag = new System.Windows.Forms.MenuItem();
            this.mnuReadTags = new System.Windows.Forms.MenuItem();
            this.mnuEventLogging = new System.Windows.Forms.MenuItem();
            this.mnuFreeCMDs = new System.Windows.Forms.MenuItem();
            this.txtBarcodeData = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTagID = new System.Windows.Forms.TextBox();
            this.txtTagData = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCMD = new System.Windows.Forms.TextBox();
            this.lblCMD = new System.Windows.Forms.Label();
            this.btnCMD = new System.Windows.Forms.Button();
            this.txtBoxID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtTagDataStr = new System.Windows.Forms.TextBox();
            this.mnuEnableBOXID = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            this.mainMenu1.MenuItems.Add(this.menuItem2);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.mnuRestart);
            this.menuItem1.MenuItems.Add(this.mnuDisconnectReader);
            this.menuItem1.MenuItems.Add(this.mnuConnectReader);
            this.menuItem1.MenuItems.Add(this.mnuExit);
            this.menuItem1.Text = "File";
            // 
            // mnuRestart
            // 
            this.mnuRestart.Text = "Restart";
            this.mnuRestart.Click += new System.EventHandler(this.mnuRestart_Click);
            // 
            // mnuDisconnectReader
            // 
            this.mnuDisconnectReader.Enabled = false;
            this.mnuDisconnectReader.Text = "Disconnect Reader";
            this.mnuDisconnectReader.Click += new System.EventHandler(this.mnuDisconnectReader_Click);
            // 
            // mnuConnectReader
            // 
            this.mnuConnectReader.Text = "Connect Reader";
            this.mnuConnectReader.Click += new System.EventHandler(this.mnuConnectReader_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.mnuEnableLogging);
            this.menuItem2.MenuItems.Add(this.mnuWriteTag);
            this.menuItem2.MenuItems.Add(this.mnuReadTags);
            this.menuItem2.MenuItems.Add(this.mnuEventLogging);
            this.menuItem2.MenuItems.Add(this.mnuFreeCMDs);
            this.menuItem2.MenuItems.Add(this.mnuEnableBOXID);
            this.menuItem2.Text = "Options";
            // 
            // mnuEnableLogging
            // 
            this.mnuEnableLogging.Text = "BRI Logging";
            this.mnuEnableLogging.Click += new System.EventHandler(this.mnuEnableLogging_Click);
            // 
            // mnuWriteTag
            // 
            this.mnuWriteTag.Text = "Write TAG";
            this.mnuWriteTag.Click += new System.EventHandler(this.mnuWriteTag_Click);
            // 
            // mnuReadTags
            // 
            this.mnuReadTags.Text = "Read TAGs";
            this.mnuReadTags.Click += new System.EventHandler(this.mnuReadTags_Click);
            // 
            // mnuEventLogging
            // 
            this.mnuEventLogging.Text = "EventLogging";
            this.mnuEventLogging.Click += new System.EventHandler(this.mnuEventLogging_Click);
            // 
            // mnuFreeCMDs
            // 
            this.mnuFreeCMDs.Text = "Free CMDs";
            this.mnuFreeCMDs.Click += new System.EventHandler(this.menuFreeCMDs_Click);
            // 
            // txtBarcodeData
            // 
            this.txtBarcodeData.Location = new System.Drawing.Point(70, 3);
            this.txtBarcodeData.Name = "txtBarcodeData";
            this.txtBarcodeData.ReadOnly = true;
            this.txtBarcodeData.Size = new System.Drawing.Size(167, 21);
            this.txtBarcodeData.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 18);
            this.label1.Text = "Barcode:";
            // 
            // txtTagID
            // 
            this.txtTagID.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.txtTagID.Location = new System.Drawing.Point(70, 30);
            this.txtTagID.Name = "txtTagID";
            this.txtTagID.ReadOnly = true;
            this.txtTagID.Size = new System.Drawing.Size(167, 19);
            this.txtTagID.TabIndex = 1;
            this.txtTagID.TextChanged += new System.EventHandler(this.txtTagID_TextChanged);
            // 
            // txtTagData
            // 
            this.txtTagData.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.txtTagData.Location = new System.Drawing.Point(70, 49);
            this.txtTagData.Name = "txtTagData";
            this.txtTagData.ReadOnly = true;
            this.txtTagData.Size = new System.Drawing.Size(167, 19);
            this.txtTagData.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label3.Location = new System.Drawing.Point(3, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 18);
            this.label3.Text = "TAG data:";
            // 
            // txtStatus
            // 
            this.txtStatus.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.txtStatus.Location = new System.Drawing.Point(3, 144);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(234, 23);
            this.txtStatus.TabIndex = 3;
            this.txtStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(3, 173);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(234, 92);
            this.txtLog.TabIndex = 7;
            this.txtLog.Text = "Please select File-Connect";
            this.txtLog.WordWrap = false;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.label4.Location = new System.Drawing.Point(3, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 18);
            this.label4.Text = "TAG:";
            // 
            // txtCMD
            // 
            this.txtCMD.Location = new System.Drawing.Point(46, 117);
            this.txtCMD.Name = "txtCMD";
            this.txtCMD.Size = new System.Drawing.Size(163, 21);
            this.txtCMD.TabIndex = 11;
            // 
            // lblCMD
            // 
            this.lblCMD.Location = new System.Drawing.Point(3, 117);
            this.lblCMD.Name = "lblCMD";
            this.lblCMD.Size = new System.Drawing.Size(37, 18);
            this.lblCMD.Text = "cmd:";
            // 
            // btnCMD
            // 
            this.btnCMD.Location = new System.Drawing.Point(215, 117);
            this.btnCMD.Name = "btnCMD";
            this.btnCMD.Size = new System.Drawing.Size(22, 21);
            this.btnCMD.TabIndex = 14;
            this.btnCMD.Text = "do";
            this.btnCMD.Click += new System.EventHandler(this.btnCMD_Click);
            // 
            // txtBoxID
            // 
            this.txtBoxID.Location = new System.Drawing.Point(70, 90);
            this.txtBoxID.Multiline = true;
            this.txtBoxID.Name = "txtBoxID";
            this.txtBoxID.Size = new System.Drawing.Size(75, 21);
            this.txtBoxID.TabIndex = 1;
            this.txtBoxID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 18);
            this.label2.Text = "BOX id:";
            // 
            // txtTagDataStr
            // 
            this.txtTagDataStr.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
            this.txtTagDataStr.Location = new System.Drawing.Point(70, 68);
            this.txtTagDataStr.Name = "txtTagDataStr";
            this.txtTagDataStr.ReadOnly = true;
            this.txtTagDataStr.Size = new System.Drawing.Size(167, 19);
            this.txtTagDataStr.TabIndex = 1;
            // 
            // mnuEnableBOXID
            // 
            this.mnuEnableBOXID.Text = "BOX ID editing";
            this.mnuEnableBOXID.Click += new System.EventHandler(this.mnuEnableBOXID_Click);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.btnCMD);
            this.Controls.Add(this.txtCMD);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.lblCMD);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBoxID);
            this.Controls.Add(this.txtTagDataStr);
            this.Controls.Add(this.txtTagData);
            this.Controls.Add(this.txtTagID);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBarcodeData);
            this.Menu = this.mainMenu1;
            this.Name = "mainForm";
            this.Text = "IP4scanNtag DemoApp";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem mnuRestart;
        private System.Windows.Forms.MenuItem mnuExit;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem mnuEnableLogging;
        private System.Windows.Forms.TextBox txtBarcodeData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTagID;
        private System.Windows.Forms.TextBox txtTagData;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuItem mnuDisconnectReader;
        private System.Windows.Forms.MenuItem mnuConnectReader;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.MenuItem mnuWriteTag;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.MenuItem mnuReadTags;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCMD;
        private System.Windows.Forms.Label lblCMD;
        private System.Windows.Forms.Button btnCMD;
        private System.Windows.Forms.MenuItem mnuEventLogging;
        private System.Windows.Forms.MenuItem mnuFreeCMDs;
        private System.Windows.Forms.TextBox txtBoxID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtTagDataStr;
        private System.Windows.Forms.MenuItem mnuEnableBOXID;
    }
}

