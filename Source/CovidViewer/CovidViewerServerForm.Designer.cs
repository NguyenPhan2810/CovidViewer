namespace CovidViewerServer
{
    partial class CovidViewerServerForm
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.buttonFetch = new System.Windows.Forms.Button();
            this.labelIp = new System.Windows.Forms.Label();
            this.textBoxIp = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 9);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(37, 13);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Status";
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Location = new System.Drawing.Point(78, 6);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Size = new System.Drawing.Size(343, 20);
            this.textBoxStatus.TabIndex = 1;
            this.textBoxStatus.TabStop = false;
            // 
            // buttonFetch
            // 
            this.buttonFetch.Location = new System.Drawing.Point(346, 58);
            this.buttonFetch.Name = "buttonFetch";
            this.buttonFetch.Size = new System.Drawing.Size(75, 23);
            this.buttonFetch.TabIndex = 2;
            this.buttonFetch.Text = "Fetch";
            this.buttonFetch.UseVisualStyleBackColor = true;
            this.buttonFetch.Click += new System.EventHandler(this.buttonFetch_Click);
            // 
            // labelIp
            // 
            this.labelIp.AutoSize = true;
            this.labelIp.Location = new System.Drawing.Point(12, 35);
            this.labelIp.Name = "labelIp";
            this.labelIp.Size = new System.Drawing.Size(16, 13);
            this.labelIp.TabIndex = 4;
            this.labelIp.Text = "Ip";
            // 
            // textBoxIp
            // 
            this.textBoxIp.Location = new System.Drawing.Point(78, 32);
            this.textBoxIp.Name = "textBoxIp";
            this.textBoxIp.ReadOnly = true;
            this.textBoxIp.Size = new System.Drawing.Size(343, 20);
            this.textBoxIp.TabIndex = 6;
            // 
            // CovidViewerServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 94);
            this.Controls.Add(this.textBoxIp);
            this.Controls.Add(this.labelIp);
            this.Controls.Add(this.buttonFetch);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.labelStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "CovidViewerServerForm";
            this.Text = "Covid Viewer Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CovidViewerServerForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.CovidViewerServerForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonFetch;
        private System.Windows.Forms.Label labelIp;
        private System.Windows.Forms.TextBox textBoxIp;
    }
}

