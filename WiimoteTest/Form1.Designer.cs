namespace WiimoteWhiteboard
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
            this.pbBattery = new System.Windows.Forms.ProgressBar();
            this.lblBattery = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cbCursorControl = new System.Windows.Forms.CheckBox();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.lblIRvisible = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.lblSmoothing = new System.Windows.Forms.Label();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pbTrackingUtil = new System.Windows.Forms.ProgressBar();
            this.lblTrackingUtil = new System.Windows.Forms.Label();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbBattery
            // 
            this.pbBattery.Location = new System.Drawing.Point(8, 20);
            this.pbBattery.Maximum = 200;
            this.pbBattery.Name = "pbBattery";
            this.pbBattery.Size = new System.Drawing.Size(92, 13);
            this.pbBattery.Step = 1;
            this.pbBattery.TabIndex = 6;
            // 
            // lblBattery
            // 
            this.lblBattery.AutoSize = true;
            this.lblBattery.Location = new System.Drawing.Point(103, 20);
            this.lblBattery.Name = "lblBattery";
            this.lblBattery.Size = new System.Drawing.Size(13, 13);
            this.lblBattery.TabIndex = 9;
            this.lblBattery.Text = "--";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pbBattery);
            this.groupBox4.Controls.Add(this.lblBattery);
            this.groupBox4.Location = new System.Drawing.Point(10, 8);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(133, 44);
            this.groupBox4.TabIndex = 21;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Wiimote Battery";
            // 
            // cbCursorControl
            // 
            this.cbCursorControl.AutoSize = true;
            this.cbCursorControl.Location = new System.Drawing.Point(18, 128);
            this.cbCursorControl.Margin = new System.Windows.Forms.Padding(2);
            this.cbCursorControl.Name = "cbCursorControl";
            this.cbCursorControl.Size = new System.Drawing.Size(92, 17);
            this.cbCursorControl.TabIndex = 22;
            this.cbCursorControl.Text = "Cursor Control";
            this.cbCursorControl.UseVisualStyleBackColor = true;
            this.cbCursorControl.CheckedChanged += new System.EventHandler(this.cbCursorControl_CheckedChanged);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Location = new System.Drawing.Point(9, 218);
            this.btnCalibrate.Margin = new System.Windows.Forms.Padding(2);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(135, 62);
            this.btnCalibrate.TabIndex = 24;
            this.btnCalibrate.Text = "Calibrate Location (Wiimote A)";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // lblIRvisible
            // 
            this.lblIRvisible.AutoSize = true;
            this.lblIRvisible.Location = new System.Drawing.Point(15, 107);
            this.lblIRvisible.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblIRvisible.Name = "lblIRvisible";
            this.lblIRvisible.Size = new System.Drawing.Size(80, 13);
            this.lblIRvisible.TabIndex = 25;
            this.lblIRvisible.Text = "Visible IR dots: ";
            this.lblIRvisible.Click += new System.EventHandler(this.lblIRvisible_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(8, 169);
            this.trackBar1.Maximum = 20;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(135, 45);
            this.trackBar1.TabIndex = 27;
            this.trackBar1.Value = 4;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // lblSmoothing
            // 
            this.lblSmoothing.AutoSize = true;
            this.lblSmoothing.Location = new System.Drawing.Point(15, 153);
            this.lblSmoothing.Name = "lblSmoothing";
            this.lblSmoothing.Size = new System.Drawing.Size(69, 13);
            this.lblSmoothing.TabIndex = 28;
            this.lblSmoothing.Text = "Smoothing: 4";
            this.lblSmoothing.Click += new System.EventHandler(this.label1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pbTrackingUtil);
            this.groupBox1.Controls.Add(this.lblTrackingUtil);
            this.groupBox1.Location = new System.Drawing.Point(8, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 44);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tracking Utilization";
            // 
            // pbTrackingUtil
            // 
            this.pbTrackingUtil.Location = new System.Drawing.Point(8, 20);
            this.pbTrackingUtil.Name = "pbTrackingUtil";
            this.pbTrackingUtil.Size = new System.Drawing.Size(94, 13);
            this.pbTrackingUtil.Step = 1;
            this.pbTrackingUtil.TabIndex = 6;
            // 
            // lblTrackingUtil
            // 
            this.lblTrackingUtil.AutoSize = true;
            this.lblTrackingUtil.Location = new System.Drawing.Point(105, 20);
            this.lblTrackingUtil.Name = "lblTrackingUtil";
            this.lblTrackingUtil.Size = new System.Drawing.Size(13, 13);
            this.lblTrackingUtil.TabIndex = 9;
            this.lblTrackingUtil.Text = "--";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(155, 287);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblSmoothing);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.lblIRvisible);
            this.Controls.Add(this.btnCalibrate);
            this.Controls.Add(this.cbCursorControl);
            this.Controls.Add(this.groupBox4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Wiimote Whiteboard";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.ProgressBar pbBattery;
        private System.Windows.Forms.Label lblBattery;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox cbCursorControl;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Label lblIRvisible;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Label lblSmoothing;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar pbTrackingUtil;
        private System.Windows.Forms.Label lblTrackingUtil;
	}
}

