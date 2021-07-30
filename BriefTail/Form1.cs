using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BriefTail
{
    public partial class Form1 : Form
    {
        private string FileName = "";
        private const int MaxLine = 100;
        private long CurrentPosition = 0;

        public Form1()
        {
            InitializeComponent();
            Timer refreshTimer = new Timer();
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Interval = 500;
            refreshTimer.Start();
            TailBox.AllowDrop = true;
            TailBox.DragDrop += TailBox_DragDrop;
            TailBox.DragEnter += TailBox_DragEnter;
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (FileName != "" && File.Exists(FileName))
            {
                LimitLines(MaxLine);
                ShowFile();
            }
        }

        private void MenuOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "所有文件|*.*";
            ofd.ValidateNames = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileName = ofd.FileName;
                CurrentPosition = 0;
                ShowFile();
            }
        }

        private void TailBox_SizeChanged(object sender, EventArgs e)
        {
        }

        private void TailBox_DragDrop(object sender, DragEventArgs e)
        {
            FileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.TailBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            CurrentPosition = 0;
            ShowFile();
        }

        private void TailBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                this.TailBox.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ShowFile()
        {
            if (!File.Exists(FileName))
            {
                MessageBox.Show("文件不存在！");
                return;
            }

            using (FileStream fileStream = new FileStream(FileName,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (fileStream.Length == CurrentPosition)
                {
                    return;
                }

                string[] appendText = null;

                if (0 == CurrentPosition)
                {
                    fileStream.Seek(-1, SeekOrigin.End);
                    int b;
                    int lineCount = 0;
                    int currentPostion = 0;
                    do
                    {
                        b = fileStream.ReadByte();
                        currentPostion++;
                        if (b == 0xa)
                        {
                            lineCount++;
                        }
                        if (lineCount > MaxLine)
                        {
                            break;
                        }
                        fileStream.Seek(-2, SeekOrigin.Current);
                    }
                    while (-1 != b);

                    fileStream.Seek(-currentPostion, SeekOrigin.End);
                    byte[] buff = new byte[currentPostion];
                    fileStream.Read(buff, 0, currentPostion);
                    string read = Encoding.UTF8.GetString(buff, 0, currentPostion);
                    appendText = read.Split('\n');

                }
                else
                {
                    fileStream.Seek(CurrentPosition, SeekOrigin.Begin);
                    int appendLen = (int)(fileStream.Length - CurrentPosition);
                    byte[] buff = new byte[appendLen];
                    fileStream.Read(buff, 0, appendLen);
                    string read = Encoding.UTF8.GetString(buff, 0, appendLen);
                    appendText = read.Split('\n');
                }

                foreach (var line in appendText)
                {
                    AppendText(line, Color.Blue);
                }

                CurrentPosition = fileStream.Length;

            }
        }

        private void HighlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void AppendText(string text, Color color)
        {
            TailBox.SelectionStart = TailBox.TextLength;
            TailBox.SelectionLength = 0;

            TailBox.SelectionColor = color;
            TailBox.AppendText(text);
            TailBox.SelectionColor = TailBox.ForeColor;
        }

        private void LimitLines(int maxLine)
        {
            if (TailBox.Lines.Length > maxLine)
            {
                TailBox.ReadOnly = false;
                TailBox.Select(0, TailBox.GetFirstCharIndexFromLine(TailBox.Lines.Length - maxLine));
                TailBox.SelectedText = "";
                TailBox.Select(TailBox.TextLength, 0);
                TailBox.Focus();
                TailBox.Refresh();
                TailBox.ReadOnly = true;
                return;
            }

            //if (TailBox.Lines.Length > maxLine)
            //{
            //    string[] lines = new string[maxLine];
            //    Array.Copy(TailBox.Lines, TailBox.Lines.Length - maxLine, lines, 0, maxLine);
            //    TailBox.Lines = lines;
            //}
        }
    }
}
