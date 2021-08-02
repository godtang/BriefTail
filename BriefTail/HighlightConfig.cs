using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BriefTail
{
    public delegate void HighlightConfEventHandler(object sender, Dictionary<string, Color> dict);

    public partial class HighlightConfig : Form
    {
        public event HighlightConfEventHandler HighlightConf;
        private Dictionary<string, Color> HighlightDict = new Dictionary<string, Color>();

        public HighlightConfig(Dictionary<string, Color> highlightDict)
        {
            InitializeComponent();
            HighlightDict = highlightDict;
            foreach (var item in HighlightDict)
            {
                Configuration conf = new Configuration();
                conf.Delete += Conf_Delete;
                conf.MatchText = item.Key;
                conf.ShowColor = item.Value;
                flowLayoutPanel1.Controls.Add(conf);
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            Configuration conf = new Configuration();
            conf.Delete += Conf_Delete;
            flowLayoutPanel1.Controls.Add(conf);
        }

        private void Conf_Delete(object sender)
        {
            Configuration conf = sender as Configuration;
            flowLayoutPanel1.Controls.Remove(conf);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Dictionary<string, Color> highlightDict = new Dictionary<string, Color>();
            foreach (var item in flowLayoutPanel1.Controls)
            {
                if (item is Configuration conf)
                {
                    if ("" != conf.MatchText)
                    {
                        highlightDict[conf.MatchText] = conf.ShowColor;
                    }
                }
            }
            HighlightConf?.Invoke(this, highlightDict);
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
