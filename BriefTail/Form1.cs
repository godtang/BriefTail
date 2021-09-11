using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BriefTail
{
    public partial class Form1 : Form
    {
        private string FileName = "";
        private const int OpenMaxLine = 100;
        private const int TailMaxLine = 1000;
        private long CurrentPosition = 0;
        private Dictionary<string, Color> HighlightDict = new Dictionary<string, Color>();
        private JToken ConfigRoot { get; set; }
        private string ConfigFile = "";
        private SynchronizationContext context_;
        System.Windows.Forms.Timer RefreshTimer = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();
            RefreshTimer.Tick += RefreshTimer_Tick;
            RefreshTimer.Interval = 500;
            RefreshTimer.Start();
            TailBox.AllowDrop = true;
            TailBox.DragDrop += TailBox_DragDrop;
            TailBox.DragEnter += TailBox_DragEnter;


            ConfigFile = Path.Combine(
                Path.GetDirectoryName(Assembly.GetAssembly(typeof(Form1)).Location),
                "config.json");
            InitHighlight();
            context_ = SynchronizationContext.Current;
            FileName = ConfigRoot["fileHistory"].ToString();
            ShowFile();
            TailBox.HideSelection = false;
        }

        private void InitHighlight()
        {

            if (!File.Exists(ConfigFile))
            {
                ConfigRoot = JToken.Parse("{}");
            }
            else
            {
                var text = File.ReadAllText(ConfigFile, Encoding.UTF8);
                ConfigRoot = JToken.Parse(text);
            }

            var highlight = ConfigRoot["highlight"];
            foreach (var item in highlight)
            {
                JProperty temp = item as JProperty;
                HighlightDict[temp.Name.ToString()] = Color.FromArgb((int)temp.Value);
            }

            SortHighlightTable();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            context_.Post((object status) =>
            {
                if (FileName != "" && File.Exists(FileName))
                {
                    LimitLines(TailMaxLine);
                    ShowFile();
                }
                else
                {
                    TailBox.Clear();
                    CurrentPosition = 0;
                }
            }, null);
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
                if (FileName != ofd.FileName)
                {
                    TailBox.Clear();
                    CurrentPosition = 0;
                    FileName = ofd.FileName;
                    ConfigRoot["fileHistory"] = FileName;
                    SaveConfig();
                    CurrentPosition = 0;
                    ShowFile();
                }
            }
        }

        private void TailBox_SizeChanged(object sender, EventArgs e)
        {
        }

        private void TailBox_DragDrop(object sender, DragEventArgs e)
        {
            if (FileName != ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString())
            {
                TailBox.Clear();
                CurrentPosition = 0;
                FileName = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                ConfigRoot["fileHistory"] = FileName;
                SaveConfig();
                this.TailBox.Cursor = System.Windows.Forms.Cursors.IBeam;
                ShowFile();
            }
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
                //MessageBox.Show("文件不存在！");
                Console.WriteLine("文件不存在！");
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
                        if (lineCount > OpenMaxLine)
                        {
                            break;
                        }
                        if (1 == fileStream.Position)
                        {
                            break;
                        }
                        else
                        {
                            fileStream.Seek(-2, SeekOrigin.Current);
                        }
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

                for (int i = 0; i < appendText.Length; i++)
                {
                    string line = appendText[i];
                    if (i != appendText.Length - 1)
                    {
                        line = line + "\n";
                    }
                    AppendText(line);
                }


                CurrentPosition = fileStream.Length;

            }
        }

        private void HighlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HighlightConfig dialog = new HighlightConfig(HighlightDict);
            dialog.HighlightConf += Dialog_HighlightConf;
            dialog.ShowDialog();
        }

        private void Dialog_HighlightConf(object sender, Dictionary<string, Color> dict)
        {
            HighlightDict = dict;
            SortHighlightTable();
            RefreshHighlight();
            SaveConfig();
        }

        private void RefreshHighlight()
        {
            string all = TailBox.Text;
            TailBox.Clear();
            string[] appendText = all.Split('\n');

            for (int i = 0; i < appendText.Length; i++)
            {
                string line = appendText[i];
                if (i != appendText.Length - 1)
                {
                    line = line + "\n";
                }
                AppendText(line);
            }
        }

        private void AppendText(string text)
        {
            bool find = false;
            foreach (string key in HighlightDict.Keys)
            {
                if (text.IndexOf(key) >= 0)
                {
                    find = true;
                    Color c = HighlightDict[key];
                    AppendText(text, c);
                    break;
                }
            }
            if (!find)
            {
                TailBox.AppendText(text);
            }
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
                TailBox.Refresh();
                TailBox.ReadOnly = true;
                TailBox.Select(TailBox.TextLength, 0);
                TailBox.ScrollToCaret();
                return;
            }
        }

        private void SortHighlightTable()
        {
            var dicSort = from objDic in HighlightDict orderby objDic.Key.Length descending select objDic;
            Dictionary<string, Color> tempDict = new Dictionary<string, Color>();
            foreach (KeyValuePair<string, Color> kvp in dicSort)
            {
                tempDict[kvp.Key] = kvp.Value;
            }
            HighlightDict = tempDict;
        }

        private void SaveConfig()
        {
            foreach (var item in HighlightDict)
            {
                ConfigRoot["highlight"][item.Key] = item.Value.ToArgb();
            }
            File.WriteAllText(ConfigFile, ConfigRoot.ToString(), new System.Text.UTF8Encoding(false));
        }

        private void PauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PauseToolStripMenuItem.Checked = !PauseToolStripMenuItem.Checked;
            if (PauseToolStripMenuItem.Checked)
            {
                RefreshTimer.Stop();
            }
            else
            {
                RefreshTimer.Start();
            }
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TailBox.Clear();
        }
    }
}
