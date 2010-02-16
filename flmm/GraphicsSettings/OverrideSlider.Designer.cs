namespace Fomm.GraphicsSettings
{
	partial class OverrideSlider
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.nudValue = new System.Windows.Forms.NumericUpDown();
			this.ckbOverride = new System.Windows.Forms.CheckBox();
			this.tkbSlider = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.nudValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tkbSlider)).BeginInit();
			this.SuspendLayout();
			// 
			// nudValue
			// 
			this.nudValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.nudValue.Location = new System.Drawing.Point(3, 42);
			this.nudValue.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
			this.nudValue.Name = "nudValue";
			this.nudValue.Size = new System.Drawing.Size(82, 20);
			this.nudValue.TabIndex = 11;
			// 
			// ckbOverride
			// 
			this.ckbOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ckbOverride.AutoSize = true;
			this.ckbOverride.Location = new System.Drawing.Point(94, 43);
			this.ckbOverride.Name = "ckbOverride";
			this.ckbOverride.Size = new System.Drawing.Size(66, 17);
			this.ckbOverride.TabIndex = 9;
			this.ckbOverride.Text = "Override";
			this.ckbOverride.UseVisualStyleBackColor = true;
			// 
			// tkbSlider
			// 
			this.tkbSlider.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tkbSlider.BackColor = System.Drawing.SystemColors.Window;
			this.tkbSlider.Location = new System.Drawing.Point(3, 3);
			this.tkbSlider.Minimum = 2;
			this.tkbSlider.Name = "tkbSlider";
			this.tkbSlider.Size = new System.Drawing.Size(208, 45);
			this.tkbSlider.TabIndex = 8;
			this.tkbSlider.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
			this.tkbSlider.Value = 2;
			this.tkbSlider.Scroll += new System.EventHandler(this.tkbSlider_Scroll);
			// 
			// OverrideSlider
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.nudValue);
			this.Controls.Add(this.ckbOverride);
			this.Controls.Add(this.tkbSlider);
			this.Name = "OverrideSlider";
			this.Size = new System.Drawing.Size(216, 67);
			((System.ComponentModel.ISupportInitialize)(this.nudValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tkbSlider)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.NumericUpDown nudValue;
		private System.Windows.Forms.CheckBox ckbOverride;
		private System.Windows.Forms.TrackBar tkbSlider;
	}
}
