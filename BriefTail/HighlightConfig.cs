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
            conf.Location = new Point(10, 25 * Count);
            this.Controls.Add(conf);
            Count++;
        }
    }
}
