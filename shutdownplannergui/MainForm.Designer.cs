namespace ShutdownPlannerGUI {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonAbortSubmit = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonShutdownSubmit = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxShutdownTotalSeconds = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxShutdownSeconds = new System.Windows.Forms.TextBox();
            this.textBoxShutdownMinutes = new System.Windows.Forms.TextBox();
            this.textBoxShutdownHours = new System.Windows.Forms.TextBox();
            this.checkBoxShutdownForce = new System.Windows.Forms.CheckBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonAbortSubmit);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(10, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(604, 49);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Abort Shutdown";
            // 
            // buttonAbortSubmit
            // 
            this.buttonAbortSubmit.Location = new System.Drawing.Point(6, 19);
            this.buttonAbortSubmit.Name = "buttonAbortSubmit";
            this.buttonAbortSubmit.Size = new System.Drawing.Size(75, 23);
            this.buttonAbortSubmit.TabIndex = 13;
            this.buttonAbortSubmit.Text = "Submit";
            this.buttonAbortSubmit.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.buttonShutdownSubmit);
            this.groupBox2.Controls.Add(this.checkBoxShutdownForce);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.textBoxShutdownTotalSeconds);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBoxShutdownSeconds);
            this.groupBox2.Controls.Add(this.textBoxShutdownMinutes);
            this.groupBox2.Controls.Add(this.textBoxShutdownHours);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(10, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(604, 124);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Plan Shutdown";
            // 
            // buttonShutdownSubmit
            // 
            this.buttonShutdownSubmit.Location = new System.Drawing.Point(6, 94);
            this.buttonShutdownSubmit.Name = "buttonShutdownSubmit";
            this.buttonShutdownSubmit.Size = new System.Drawing.Size(75, 23);
            this.buttonShutdownSubmit.TabIndex = 20;
            this.buttonShutdownSubmit.Text = "Submit";
            this.buttonShutdownSubmit.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(353, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Seconds";
            // 
            // textBoxShutdownTotalSeconds
            // 
            this.textBoxShutdownTotalSeconds.Location = new System.Drawing.Point(175, 45);
            this.textBoxShutdownTotalSeconds.Name = "textBoxShutdownTotalSeconds";
            this.textBoxShutdownTotalSeconds.Size = new System.Drawing.Size(172, 20);
            this.textBoxShutdownTotalSeconds.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(256, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(10, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(159, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = ":";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Shutdown in:";
            // 
            // textBoxShutdownSeconds
            // 
            this.textBoxShutdownSeconds.Location = new System.Drawing.Point(272, 19);
            this.textBoxShutdownSeconds.Name = "textBoxShutdownSeconds";
            this.textBoxShutdownSeconds.Size = new System.Drawing.Size(75, 20);
            this.textBoxShutdownSeconds.TabIndex = 13;
            // 
            // textBoxShutdownMinutes
            // 
            this.textBoxShutdownMinutes.Location = new System.Drawing.Point(175, 19);
            this.textBoxShutdownMinutes.Name = "textBoxShutdownMinutes";
            this.textBoxShutdownMinutes.Size = new System.Drawing.Size(75, 20);
            this.textBoxShutdownMinutes.TabIndex = 12;
            // 
            // textBoxShutdownHours
            // 
            this.textBoxShutdownHours.Location = new System.Drawing.Point(78, 19);
            this.textBoxShutdownHours.Name = "textBoxShutdownHours";
            this.textBoxShutdownHours.Size = new System.Drawing.Size(75, 20);
            this.textBoxShutdownHours.TabIndex = 11;
            // 
            // checkBoxShutdownForce
            // 
            this.checkBoxShutdownForce.AutoSize = true;
            this.checkBoxShutdownForce.Checked = true;
            this.checkBoxShutdownForce.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShutdownForce.Location = new System.Drawing.Point(6, 71);
            this.checkBoxShutdownForce.Name = "checkBoxShutdownForce";
            this.checkBoxShutdownForce.Size = new System.Drawing.Size(53, 17);
            this.checkBoxShutdownForce.TabIndex = 19;
            this.checkBoxShutdownForce.Text = "Force";
            this.checkBoxShutdownForce.UseVisualStyleBackColor = true;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxOutput.Location = new System.Drawing.Point(10, 189);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(604, 488);
            this.textBoxOutput.TabIndex = 15;
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(10, 130);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 5);
            this.panel1.TabIndex = 16;
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(10, 184);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(604, 5);
            this.panel2.TabIndex = 17;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 681);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox2);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(10, 6, 10, 4);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Shutdown Planner GUI";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonAbortSubmit;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonShutdownSubmit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxShutdownTotalSeconds;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxShutdownSeconds;
        private System.Windows.Forms.TextBox textBoxShutdownMinutes;
        private System.Windows.Forms.TextBox textBoxShutdownHours;
        private System.Windows.Forms.CheckBox checkBoxShutdownForce;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}

