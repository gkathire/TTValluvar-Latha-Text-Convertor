using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Editor
{
    public partial class Form1 : Form
    {
        public enum ScrollBarType : uint
        {
            SbHorz = 0,
            SbVert = 1,
            SbCtl = 2,
            SbBoth = 3
        }

        public enum Message : uint
        {
            WM_VSCROLL = 0x0115
        }

        public enum ScrollBarCommands : uint
        {
            SB_THUMBPOSITION = 4
        }

        [DllImport("User32.dll")]
        public extern static int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("User32.dll")]
        public extern static int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public Form1()
        {
            InitializeComponent();

            richTextBox1.Text = richTextBox1.Text.Trim();
            richTextBox2.Text = richTextBox2.Text.Trim();

            //
            var lines = richTextBox1.Lines.ToList();
            var linesToCopy = new List<string>();
            foreach (var line in lines)
            {
                if (line.Trim() == "")
                    continue;
                linesToCopy.Add(line);
            }
            richTextBox1.Lines = linesToCopy.ToArray();

            //
            lines = richTextBox2.Lines.ToList();
            linesToCopy.Clear();
            foreach (var line in lines)
            {
                if (line.Trim() == "")
                    continue;
                linesToCopy.Add(line);
            }
            richTextBox2.Lines = linesToCopy.ToArray();

            Button1_Click(null, null);

            UpdateLineCounts();
        }

        private void ListView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                // Draw the standard header background.
                e.DrawBackground();
                var fontName = "TM-TTValluvar";
                if (e.ItemIndex == 1)
                {
                    fontName = "Latha";
                }

                // Draw the header text.
                using (Font headerFont =
                            new Font(fontName, 10, FontStyle.Bold)) //Font size!!!!
                {
                    e.Graphics.DrawString(e.Item.Text, headerFont,
                        Brushes.Black, e.Bounds, sf);
                }
            }
            return;
        }

        private void UpdateLineCounts()
        {
            int cursorPosition = richTextBox1.SelectionStart;
            int lineIndex = richTextBox1.GetLineFromCharIndex(cursorPosition) + 1;
            lbValluvarLine.Text = "Line - " + lineIndex.ToString();

            cursorPosition = richTextBox2.SelectionStart;
            lineIndex = richTextBox2.GetLineFromCharIndex(cursorPosition) + 1;
            lbLathaLine.Text = "Line - " + lineIndex.ToString();
        }

        private void RichTextBox1_VScroll(object sender, EventArgs e)
        {
                int nPos = GetScrollPos(richTextBox1.Handle, (int)ScrollBarType.SbVert);
                nPos <<= 16;
                uint wParam = (uint)ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
                SendMessage(richTextBox2.Handle, (int)Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));

            UpdateLineCounts();
        }

        private void RichTextBox2_VScroll(object sender, EventArgs e)
        {
            UpdateLineCounts();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //listView1.Items.Clear();

            if (richTextBox1.Lines.Count() != richTextBox2.Lines.Count())
            {
                MessageBox.Show($"Line count does not match - richTextBox1 = {richTextBox1.Lines.Count()}  - richTextBox2 = {richTextBox2.Lines.Count()}, adjust the characters properly");
                //return;
            }

            if (richTextBox2.Lines.Count() > richTextBox1.Lines.Count())
            {
                var lines = richTextBox1.Lines.ToList();
                for (int i = 0; i <  richTextBox2.Lines.Count() - richTextBox1.Lines.Count();++i)
                {
                    lines.Add("");
                }
                richTextBox1.Lines = lines.ToArray();
            }
            else if (richTextBox1.Lines.Count() > richTextBox2.Lines.Count())
            {
                var lines = richTextBox2.Lines.ToList();
                for (int i = 0; i < richTextBox1.Lines.Count() - richTextBox2.Lines.Count(); ++i)
                {
                    lines.Add("");
                }
                richTextBox2.Lines = lines.ToArray();
            }

            dataGridView1.Rows.Clear();
            //listView1.Items.Clear();
            //listView1.Font = new Font("TM -TTValluvar", 10, FontStyle.Bold);

            for(var i = 0; i < richTextBox1.Lines.Count(); ++i)
            {
                var line1 = richTextBox1.Lines[i].Trim();
                var line2 = richTextBox2.Lines[i].Trim();
                //var item = listView1.Items.Add(line1.Trim());
                //item.SubItems.Add(line2.Trim());
                //item.Font = new Font("Latha", 10, FontStyle.Bold);
                this.dataGridView1.Rows.Add(line1, line2);
                this.dataGridView1.Rows[i].HeaderCell.Value = String.Format("{0}", i + 1);
            }

            //ConvertText();
        }

        private void ListView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            //
        }

        private void RichTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void RichTextBox2_SelectionChanged(object sender, EventArgs e)
        {
            UpdateLineCounts();
        }

        private void RichTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateLineCounts();
        }

        private void RichTextBox3_TextChanged(object sender, EventArgs e)
        {
        }

        private void ConvertText()
        {
            var symbols = new List<KeyValuePair<string, string>>();

            for (var i = 0; i < dataGridView1.RowCount; ++i)
            {
                var val0 = dataGridView1.Rows[i].Cells[0].Value;
                var val1 = dataGridView1.Rows[i].Cells[1].Value;
                var str0 = "";
                var str1 = "";

                if (val0 != null)
                    str0 = val0.ToString().Trim();

                if (val1 != null)
                    str1 = val1.ToString().Trim();

                symbols.Add(new KeyValuePair<string, string>(str0, str1));                
            }

            symbols =  symbols.OrderByDescending(x => x.Key.Length).ToList();

            var modifiedLines = new List<string>();

            progressBar1.Minimum = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = richTextBox3.Lines.Count();

            foreach (var line in richTextBox3.Lines)
            {
                Application.DoEvents();
                var modifiedLine = line;
                foreach (var sym in symbols)
                {
                    if (sym.Key == "")
                        continue;

                    modifiedLine = modifiedLine.Replace(sym.Key, sym.Value);
                }
                modifiedLines.Add(modifiedLine);
                progressBar1.Value = progressBar1.Value + 1;
            }

            richTextBox4.Lines = modifiedLines.ToArray();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            progressBar1.Visible = true;
            ConvertText();
            progressBar1.Visible = false;
            MessageBox.Show("Text is converted");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            richTextBox4.SelectAll();
            richTextBox4.Copy();
            MessageBox.Show("Converted text is copied to clipboard");
        }
    }
}
