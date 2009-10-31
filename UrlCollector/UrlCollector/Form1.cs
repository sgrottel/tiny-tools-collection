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
            this.checkBox1_CheckedChanged(null, null);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            this.timer1.Enabled = this.checkBox1.Checked;
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

    }

}
