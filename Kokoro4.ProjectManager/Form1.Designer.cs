namespace Kokoro4.ProjectManager
{
    partial class Form1
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.editorBtn = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.openPrjtBtn = new System.Windows.Forms.Button();
            this.settingBtn = new System.Windows.Forms.Button();
            this.newPrjtBtn = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.folderBrowserDialog2 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Location = new System.Drawing.Point(13, 38);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(414, 281);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Active Projects:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.editorBtn);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 355);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(415, 206);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Project Actions";
            // 
            // editorBtn
            // 
            this.editorBtn.Location = new System.Drawing.Point(7, 44);
            this.editorBtn.Name = "editorBtn";
            this.editorBtn.Size = new System.Drawing.Size(102, 23);
            this.editorBtn.TabIndex = 1;
            this.editorBtn.Text = "Editor";
            this.editorBtn.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(7, 20);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(174, 17);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "Background Content Processor";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // openPrjtBtn
            // 
            this.openPrjtBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.openPrjtBtn.Location = new System.Drawing.Point(13, 326);
            this.openPrjtBtn.Name = "openPrjtBtn";
            this.openPrjtBtn.Size = new System.Drawing.Size(108, 23);
            this.openPrjtBtn.TabIndex = 3;
            this.openPrjtBtn.Text = "Open Project";
            this.openPrjtBtn.UseVisualStyleBackColor = true;
            this.openPrjtBtn.Click += new System.EventHandler(this.openPrjtBtn_Click);
            // 
            // settingBtn
            // 
            this.settingBtn.Location = new System.Drawing.Point(352, 326);
            this.settingBtn.Name = "settingBtn";
            this.settingBtn.Size = new System.Drawing.Size(75, 23);
            this.settingBtn.TabIndex = 4;
            this.settingBtn.Text = "Settings";
            this.settingBtn.UseVisualStyleBackColor = true;
            // 
            // newPrjtBtn
            // 
            this.newPrjtBtn.Location = new System.Drawing.Point(128, 326);
            this.newPrjtBtn.Name = "newPrjtBtn";
            this.newPrjtBtn.Size = new System.Drawing.Size(101, 23);
            this.newPrjtBtn.TabIndex = 5;
            this.newPrjtBtn.Text = "New Project";
            this.newPrjtBtn.UseVisualStyleBackColor = true;
            this.newPrjtBtn.Click += new System.EventHandler(this.newPrjtBtn_Click);
            // 
            // folderBrowserDialog2
            // 
            this.folderBrowserDialog2.ShowNewFolderButton = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 573);
            this.Controls.Add(this.newPrjtBtn);
            this.Controls.Add(this.settingBtn);
            this.Controls.Add(this.openPrjtBtn);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Kokoro4 Project Manager";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button openPrjtBtn;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button editorBtn;
        private System.Windows.Forms.Button settingBtn;
        private System.Windows.Forms.Button newPrjtBtn;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog2;
    }
}

