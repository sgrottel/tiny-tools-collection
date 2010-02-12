using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UrlCollector {

    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
            this.toolStripButton1_CheckedChanged(null, null);
            this.Icon = Properties.Resources.UrlCollector;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (Clipboard.ContainsText()) {
                string text = Clipboard.GetText();
                if ((text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || text.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                        || text.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
                    && (!text.Contains('\n'))) {
                    this.richTextBox1.AppendText(text + "\n");
                    Clipboard.Clear();
                }
            }
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e) {
            ToolStripSplitButton tssb = sender as ToolStripSplitButton;
            if (tssb == null) return;
            tssb.ShowDropDown();
        }

        private void toolStripButton2_Click(object sender, EventArgs e) {
            this.richTextBox1.Clear();
        }

        private void toolStripButton1_CheckedChanged(object sender, EventArgs e) {
            this.timer1.Enabled = this.toolStripButton1.Checked;
        }

        private void toolStripButton5_Click(object sender, EventArgs e) {
            this.richTextBox1.Paste(DataFormats.Text);
        }

        private void copySelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.richTextBox1.Copy();
        }

        private void cutSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.richTextBox1.Cut();
        }

    }

}
