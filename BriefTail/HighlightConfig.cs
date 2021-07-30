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
    public partial class HighlightConfig : Form
    {
        int Count = 0;

        public HighlightConfig()
        {
            InitializeComponent();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            Configuration conf = new Configuration();
            conf.Delete += Conf_Delete;
            flowLayoutPanel1.Controls.Add(conf);
            Count++;
        }

        private void Conf_Delete(object sender)
        {
            Configuration conf = sender as Configuration;
            flowLayoutPanel1.Controls.Remove(conf);
        }
    }
}
