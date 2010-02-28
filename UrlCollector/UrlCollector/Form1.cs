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

        bool dirty = false;

        public Form1() {
            InitializeComponent();
            this.toolStripButton1_CheckedChanged(null, null);
            this.Icon = Properties.Resources.UrlCollector;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            int i = 0;
            try {
                i = (int)this.toolStripButton1.Tag;
            }catch {
            }
            switch (i) {
                case 1:
                    this.toolStripButton1.Image = Properties.Resources.gear2;
                    this.toolStripButton1.Tag = 2;
                    break;
                case 2:
                    this.toolStripButton1.Image = Properties.Resources.gear3;
                    this.toolStripButton1.Tag = 3;
                    break;
                default:
                    this.toolStripButton1.Image = Properties.Resources.gear1;
                    this.toolStripButton1.Tag = 1;
                    break;
            }
            if (Clipboard.ContainsText()) {
                string text = Clipboard.GetText();
                if ((text.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                        || text.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                        || text.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
                    && (!text.Contains('\n'))) {
                    this.richTextBox1.AppendText(text + "\n");
                    this.dirty = true;
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
            this.toolStripButton1.Image = this.toolStripButton1.Checked
                ? Properties.Resources.gear1
                : Properties.Resources.NoAction;
            this.toolStripButton1.Tag = null;
        }

        private void toolStripButton5_Click(object sender, EventArgs e) {
            this.richTextBox1.Paste(DataFormats.GetFormat(DataFormats.Text));
            this.dirty = true;
        }

        private void copySelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            this.richTextBox1.Copy();
        }

        private void cutSelectionToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            this.richTextBox1.Cut();
            this.dirty = true;
        }

        private void copyAllToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            int selStrt = this.richTextBox1.SelectionStart;
            int selLen = this.richTextBox1.SelectionLength;
            this.richTextBox1.SelectAll();
            this.richTextBox1.Copy();
            this.richTextBox1.Select(selStrt, selLen);
        }

        private void cutAllToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            int selStrt = this.richTextBox1.SelectionStart;
            int selLen = this.richTextBox1.SelectionLength;
            this.richTextBox1.SelectAll();
            this.richTextBox1.Cut();
            this.richTextBox1.Select(selStrt, selLen);
            this.dirty = true;
        }

        private void toolStripButton4_Click(object sender, EventArgs e) {
            this.save();
        }

        private void toolStripButton3_Click(object sender, EventArgs e) {
            if (!this.askSave()) {
                return;
            }
            try {
                this.openFileDialog1.FileName = this.saveFileDialog1.FileName;
                this.openFileDialog1.InitialDirectory = System.IO.Path.GetDirectoryName(this.openFileDialog1.FileName);
            } catch {
            }
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK) {
                this.saveFileDialog1.FileName = this.openFileDialog1.FileName;
                try {
                    this.richTextBox1.LoadFile(this.openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                    this.dirty = false;
                } catch (Exception ex) {
                    MessageBox.Show("Failed to load: " + ex.ToString());
                }
            }
        }

        private bool askSave() {
            if (!this.dirty) return true;

            while (true) {
                DialogResult dr = MessageBox.Show("Do you want to save the current text before proceeding?\nOtherwise the text will be lost",
                    Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Cancel) return false;
                if (dr == DialogResult.No) return true;
                if (dr == DialogResult.Yes) {
                    return this.save();
                }
            }
        }

        private bool save() {
            try {
                this.saveFileDialog1.InitialDirectory
                    = System.IO.Path.GetDirectoryName(this.saveFileDialog1.FileName);
            } catch {
            }
            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK) {
                this.richTextBox1.SaveFile(this.saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
                this.dirty = false;
                return true;
            }
            return false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            e.Cancel = !this.askSave();
        }

        private void copyCurrentLineToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            int selStrt = this.richTextBox1.SelectionStart;
            int selLen = this.richTextBox1.SelectionLength;
            int cidx = this.richTextBox1.GetFirstCharIndexOfCurrentLine();
            this.richTextBox1.Select(cidx,
                this.richTextBox1.Lines[this.richTextBox1.GetLineFromCharIndex(cidx)].Length);
            this.richTextBox1.Copy();
            this.richTextBox1.Select(selStrt, selLen);
            this.dirty = true;
        }

        private void cutCurrentLineToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            int selStrt = this.richTextBox1.SelectionStart;
            int selLen = this.richTextBox1.SelectionLength;
            int cidx = this.richTextBox1.GetFirstCharIndexOfCurrentLine();
            this.richTextBox1.Select(cidx,
                this.richTextBox1.Lines[this.richTextBox1.GetLineFromCharIndex(cidx)].Length);
            this.richTextBox1.Cut();
            this.richTextBox1.Select(selStrt, selLen);
            this.dirty = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e) {
            this.dirty = true;
        }

        private void copyAllAsOneLineToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            Clipboard.SetText(String.Join(" ", this.richTextBox1.Text.Split(new char[] { '\n' })));
            this.dirty = true;
        }

        private void cutAllAsSingleLineToolStripMenuItem_Click(object sender, EventArgs e) {
            this.toolStripButton1.Checked = false;
            Clipboard.SetText(String.Join(" ", this.richTextBox1.Text.Split(new char[] { '\n' })));
            this.richTextBox1.Text = string.Empty;
            this.dirty = true;
        }

    }

}
