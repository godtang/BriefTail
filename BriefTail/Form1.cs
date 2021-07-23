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
        private int MaxLine = 0;
        private long FileSize = 0;

        public Form1()
        {
            InitializeComponent();
            ReCalcMaxLine();
            Timer refreshTimer = new Timer();
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Interval = 100;
            refreshTimer.Start();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (FileName != "")
            {
                using (FileStream fileStream = new FileStream(FileName,
                    FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fileStream.Length == FileSize)
                    {
                        return;
                    }
                    fileStream.Seek(FileSize, SeekOrigin.Begin);
                    int readyRead = (int)(fileStream.Length - FileSize);
                    byte[] buffer = new byte[readyRead];
                    int readSize = fileStream.Read(buffer, 0, readyRead);
                    if (readSize != readyRead)
                    {
                        throw new Exception("read size error");
                    }
                    TailBox.AppendText(Encoding.UTF8.GetString(buffer, 0, readyRead));
                    FileSize = fileStream.Length;
                }
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
                OpenFile();
            }
        }

        private void ReCalcMaxLine()
        {
            MaxLine = TailBox.Height / (int)TailBox.Font.GetHeight();
        }

        private void TailBox_SizeChanged(object sender, EventArgs e)
        {
            ReCalcMaxLine();
        }

        private void TailBox_DragDrop(object sender, DragEventArgs e)
        {
            FileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.TailBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            OpenFile();
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

        private void OpenFile()
        {
            using (FileStream vFileStream = new FileStream(FileName,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] vBuffer = new byte[0x1000]; // 缓冲区
                int vReadLength; // 读取到的大小
                int vLineCount = 0; // 读取的行数
                int vReadCount = 0; // 读取的次数
                int vScanCount = 0; // 扫描过的字符数
                long vOffset = 0; // 向后读取的位置
                do
                {
                    vOffset = vBuffer.Length * ++vReadCount;
                    int vSpace = 0; // 偏移超出的空间
                    if (vOffset >= vFileStream.Length) // 超出范围
                    {
                        vSpace = (int)(vOffset - vFileStream.Length);
                        vOffset = vFileStream.Length;
                    }
                    vFileStream.Seek(-vOffset, SeekOrigin.End); //“SeekOrigin.End”反方向偏移读取位置

                    vReadLength = vFileStream.Read(vBuffer, 0, vBuffer.Length - vSpace);
                    #region 所读的缓冲里有多少行
                    for (int i = vReadLength - 1; i >= 0; i--)
                    {
                        if (vBuffer[i] == 10)
                        {
                            if (vScanCount > 0) vLineCount++; // #13#10为回车换行
                        }
                        vScanCount++;
                    }
                    #endregion 所读的缓冲里有多少行
                } while (vReadLength >= vBuffer.Length
                && vOffset < vFileStream.Length
                && vLineCount < MaxLine * 2);

                if (vReadCount > 1) // 读的次数超过一次，则需重分配缓冲区
                {
                    vBuffer = new byte[vScanCount];
                    vFileStream.Seek(-vScanCount, SeekOrigin.End);
                    vReadLength = vFileStream.Read(vBuffer, 0, vBuffer.Length);
                }
                FileSize = vFileStream.Length;
                TailBox.AppendText(Encoding.UTF8.GetString(vBuffer, vReadLength - vScanCount, vScanCount));
            }
        }
    }
}
