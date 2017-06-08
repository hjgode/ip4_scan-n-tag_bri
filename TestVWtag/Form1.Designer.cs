namespace TestVWtag
{
    partial class Form1
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
            this.mnuExit = new System.Windows.Forms.MenuItem();
            this.txtBarCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBarCodeClean = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtBarCodeHex = new System.Windows.Forms.TextBox();
            this.txtTagHex = new System.Windows.Forms.TextBox();
            this.btnReadTag = new System.Windows.Forms.Button();
            this.txtBoxFilter = new System.Windows.Forms.TextBox();
            this.txtTagRead = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSupplier = new System.Windows.Forms.TextBox();
            this.txtItemNumber = new System.Windows.Forms.TextBox();
            this.txtTagWrite = new System.Windows.Forms.TextBox();
            this.btnWriteTag = new System.Windows.Forms.Button();
            this.txtBoxID = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            // 
            // menuItem1
            // 
            this.menuItem1.MenuItems.Add(this.mnuExit);
            this.menuItem1.Text = "File";
            // 
            // mnuExit
            // 
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // txtBarCode
            // 
            this.txtBarCode.Location = new System.Drawing.Point(57, 4);
            this.txtBarCode.Name = "txtBarCode";
            this.txtBarCode.Size = new System.Drawing.Size(183, 21);
            this.txtBarCode.TabIndex = 0;
            this.txtBarCode.Text = "5J UN 04-997-7473 123456789";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(2, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 20);
            this.label1.Text = "Code:";
            // 
            // txtBarCodeClean
            // 
            this.txtBarCodeClean.Location = new System.Drawing.Point(57, 26);
            this.txtBarCodeClean.Name = "txtBarCodeClean";
            this.txtBarCodeClean.Size = new System.Drawing.Size(183, 21);
            this.txtBarCodeClean.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(1, 25);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(55, 22);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "ReadBC";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtBarCodeHex
            // 
            this.txtBarCodeHex.Location = new System.Drawing.Point(57, 49);
            this.txtBarCodeHex.Name = "txtBarCodeHex";
            this.txtBarCodeHex.Size = new System.Drawing.Size(183, 21);
            this.txtBarCodeHex.TabIndex = 2;
            // 
            // txtTagHex
            // 
            this.txtTagHex.Location = new System.Drawing.Point(57, 99);
            this.txtTagHex.Name = "txtTagHex";
            this.txtTagHex.Size = new System.Drawing.Size(183, 21);
            this.txtTagHex.TabIndex = 2;
            // 
            // btnReadTag
            // 
            this.btnReadTag.Location = new System.Drawing.Point(0, 195);
            this.btnReadTag.Name = "btnReadTag";
            this.btnReadTag.Size = new System.Drawing.Size(64, 22);
            this.btnReadTag.TabIndex = 4;
            this.btnReadTag.Text = "ReadTag";
            this.btnReadTag.Click += new System.EventHandler(this.btnEncodeFilter_Click);
            // 
            // txtBoxFilter
            // 
            this.txtBoxFilter.Location = new System.Drawing.Point(23, 126);
            this.txtBoxFilter.Name = "txtBoxFilter";
            this.txtBoxFilter.Size = new System.Drawing.Size(35, 21);
            this.txtBoxFilter.TabIndex = 5;
            this.txtBoxFilter.Text = "Filter";
            // 
            // txtTagRead
            // 
            this.txtTagRead.Location = new System.Drawing.Point(70, 195);
            this.txtTagRead.Name = "txtTagRead";
            this.txtTagRead.Size = new System.Drawing.Size(167, 21);
            this.txtTagRead.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 20);
            this.label2.Text = "Tag:";
            // 
            // txtSupplier
            // 
            this.txtSupplier.Location = new System.Drawing.Point(71, 126);
            this.txtSupplier.Name = "txtSupplier";
            this.txtSupplier.Size = new System.Drawing.Size(77, 21);
            this.txtSupplier.TabIndex = 5;
            this.txtSupplier.Text = "Supplier";
            // 
            // txtItemNumber
            // 
            this.txtItemNumber.Location = new System.Drawing.Point(162, 126);
            this.txtItemNumber.Name = "txtItemNumber";
            this.txtItemNumber.Size = new System.Drawing.Size(78, 21);
            this.txtItemNumber.TabIndex = 5;
            this.txtItemNumber.Text = "Filter";
            // 
            // txtTagWrite
            // 
            this.txtTagWrite.Location = new System.Drawing.Point(70, 222);
            this.txtTagWrite.Name = "txtTagWrite";
            this.txtTagWrite.Size = new System.Drawing.Size(167, 21);
            this.txtTagWrite.TabIndex = 2;
            // 
            // btnWriteTag
            // 
            this.btnWriteTag.Location = new System.Drawing.Point(0, 221);
            this.btnWriteTag.Name = "btnWriteTag";
            this.btnWriteTag.Size = new System.Drawing.Size(64, 22);
            this.btnWriteTag.TabIndex = 4;
            this.btnWriteTag.Text = "WriteTag";
            this.btnWriteTag.Click += new System.EventHandler(this.btnDecodeTagFromHex_Click);
            // 
            // txtBoxID
            // 
            this.txtBoxID.Location = new System.Drawing.Point(162, 150);
            this.txtBoxID.Name = "txtBoxID";
            this.txtBoxID.Size = new System.Drawing.Size(78, 21);
            this.txtBoxID.TabIndex = 5;
            this.txtBoxID.Text = "BoxID";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.txtBoxID);
            this.Controls.Add(this.txtItemNumber);
            this.Controls.Add(this.txtSupplier);
            this.Controls.Add(this.txtBoxFilter);
            this.Controls.Add(this.btnWriteTag);
            this.Controls.Add(this.btnReadTag);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtTagWrite);
            this.Controls.Add(this.txtTagRead);
            this.Controls.Add(this.txtTagHex);
            this.Controls.Add(this.txtBarCodeHex);
            this.Controls.Add(this.txtBarCodeClean);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBarCode);
            this.Menu = this.mainMenu1;
            this.Name = "Form1";
            this.Text = "VWtag Test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem mnuExit;
        private System.Windows.Forms.TextBox txtBarCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBarCodeClean;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox txtBarCodeHex;
        private System.Windows.Forms.TextBox txtTagHex;
        private System.Windows.Forms.Button btnReadTag;
        private System.Windows.Forms.TextBox txtBoxFilter;
        private System.Windows.Forms.TextBox txtTagRead;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSupplier;
        private System.Windows.Forms.TextBox txtItemNumber;
        private System.Windows.Forms.TextBox txtTagWrite;
        private System.Windows.Forms.Button btnWriteTag;
        private System.Windows.Forms.TextBox txtBoxID;
    }
}

