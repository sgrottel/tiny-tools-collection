using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShutdownPlannerGUI {
    public partial class MainForm : Form {

        private TimeSpan timeout;

        public MainForm() {
            InitializeComponent();
            this.Icon = Properties.Resources.Sign_06;
            this.timeout = TimeSpan.FromHours(1.0);
            SetTimeOutInGUI();
            this.updateShutdownCommand();
        }

        private void SetTimeOutInTextBox(TextBox tb, int t) {
            tb.Tag = t;
            tb.Text = t.ToString();
        }

        private void SetTimeOutInGUI() {
            SetTimeOutInTextBox(this.textBoxShutdownHours, (int)this.timeout.TotalHours);
            SetTimeOutInTextBox(this.textBoxShutdownMinutes, this.timeout.Minutes);
            SetTimeOutInTextBox(this.textBoxShutdownSeconds, this.timeout.Seconds);
            SetTimeOutInTextBox(this.textBoxShutdownTotalSeconds, (int)this.timeout.TotalSeconds);
        }

        private void buttonShutdownSubmit_Click(object sender, EventArgs e) {
            if (this.backgroundWorker.IsBusy) return;
            this.enableGuiRec(this.groupBox1.Controls, false);
            this.enableGuiRec(this.groupBox2.Controls, false);
            Application.DoEvents();

            string cmdargs = "/s";
            if (this.checkBoxShutdownForce.Checked) cmdargs += " /f";
            if (this.timeout.TotalSeconds > 0.0) cmdargs += " /t " + ((int)this.timeout.TotalSeconds).ToString();

            this.textBoxOutput.ResetText();
            this.ConsoleWriteLine("shutdown " + cmdargs);
            this.ConsoleWriteLine("");
            this.backgroundWorker.RunWorkerAsync(cmdargs);

        }

        private void buttonAbortSubmit_Click(object sender, EventArgs e) {
            if (this.backgroundWorker.IsBusy) return;

            this.enableGuiRec(this.groupBox1.Controls, false);
            this.enableGuiRec(this.groupBox2.Controls, false);
            Application.DoEvents();

            this.textBoxOutput.ResetText();
            this.ConsoleWriteLine("shutdown /a");
            this.ConsoleWriteLine("");
            this.backgroundWorker.RunWorkerAsync("/a");
        }

        private void enableGuiRec(Control.ControlCollection controlCollection, bool p) {
            foreach (Control c in controlCollection) {
                c.Enabled = p;
                if ((c.Controls != null) && (c.Controls.Count > 0)) {
                    this.enableGuiRec(c.Controls, p);
                }
            }
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            try {
                if (e.Data == null) return;
                this.ConsoleWriteLine(e.Data);
                //if (!String.IsNullOrEmpty(e.Data)) {
                //    string line = ((enc_out != null) && (enc_in != null))
                //        ? enc_out.GetString(enc_in.GetBytes(e.Data)) : e.Data;
                //    Console.Error.WriteLine(line);
                //} else Console.Error.WriteLine();
            } catch { }
        }

        private void ConsoleWriteLine(string p) {
            if (this.InvokeRequired) {
                this.Invoke(new Action<string>(this.ConsoleWriteLine), new object[] { p });
                return;
            }
            if (String.IsNullOrEmpty(p)) p = "\r\n";
            else if (!p.EndsWith("\r\n")) p += "\r\n";
            this.textBoxOutput.AppendText(p);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            string arguments = (string)e.Argument;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = arguments;
            psi.CreateNoWindow = true;
            psi.FileName = "shutdown";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Environment.CurrentDirectory;

            Process proc = new Process();
            proc.StartInfo = psi;
            proc.OutputDataReceived += proc_OutputDataReceived;
            proc.ErrorDataReceived += proc_OutputDataReceived;
            proc.EnableRaisingEvents = true;
            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            proc.WaitForExit();

            e.Result = proc.ExitCode;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.enableGuiRec(this.groupBox1.Controls, true);
            this.enableGuiRec(this.groupBox2.Controls, true);
            this.ConsoleWriteLine("");
            this.ConsoleWriteLine("Process completed (" + e.Result.ToString() + ")");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = this.backgroundWorker.IsBusy;
        }

        private void timerEditing_Tick(object sender, EventArgs e) {
            double h, m, s, ts;

            if (!double.TryParse(this.textBoxShutdownHours.Text, out h)) {
                h = (double)((int)this.textBoxShutdownHours.Tag);
            }
            if (!double.TryParse(this.textBoxShutdownMinutes.Text, out m)) {
                m = (double)((int)this.textBoxShutdownMinutes.Tag);
            }
            if (!double.TryParse(this.textBoxShutdownSeconds.Text, out s)) {
                s = (double)((int)this.textBoxShutdownSeconds.Tag);
            }

            if (!double.TryParse(this.textBoxShutdownTotalSeconds.Text, out ts)) {
                ts = (double)((int)this.textBoxShutdownTotalSeconds.Tag);
            }

            TimeSpan ts1 = TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);
            if (ts1.Milliseconds > 0) ts1 -= TimeSpan.FromMilliseconds(ts1.Milliseconds);
            TimeSpan ts2 = TimeSpan.FromSeconds(ts);
            if (ts2.Milliseconds > 0) ts2 -= TimeSpan.FromMilliseconds(ts2.Milliseconds);

            if (this.timerEditing.Tag == this.textBoxShutdownTotalSeconds) {
                this.timeout = ts2;
            } else {
                this.timeout = ts1;
            }

            this.SetTimeOutInGUI();
            this.updateShutdownCommand();
        }

        private void textBoxShutdown_TextChanged(object sender, EventArgs e) {
            this.timerEditing.Tag = sender;
            this.timerEditing.Enabled = true;
        }

        private void textBoxShutdown_Leave(object sender, EventArgs e) {
            TextBox tb = (TextBox)sender;
            double d;
            if (!double.TryParse(tb.Text, out d)) {
                tb.Text = ((int)tb.Tag).ToString();
            }
            this.timerEditing.Tag = sender;
            this.timerEditing.Enabled = false;
            this.timerEditing_Tick(this.timerEditing, e);
        }

        private void checkBoxShutdownForce_CheckedChanged(object sender, EventArgs e) {
            this.updateShutdownCommand();
        }

        private void updateShutdownCommand() {
            string cmd = "shutdown /s";
            if (this.checkBoxShutdownForce.Checked) cmd += " /f";
            if (this.timeout.TotalSeconds > 0.0) cmd += " /t " + ((int)this.timeout.TotalSeconds).ToString();
            this.labelShutdownCommand.Text = cmd;
        }

    }
}
