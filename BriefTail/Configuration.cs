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
    public delegate void DeleteEventHandler(object sender);

    public partial class Configuration : UserControl
    {
        public string MatchText
        {
            get { return text.Text; }
            set { text.Text = value; }
        }
        public Color ShowColor
        {
            get
            {
                return btnColor.BackColor;
            }
            set
            {
                btnColor.BackColor = value;
            }
        }

        public event DeleteEventHandler Delete;

        public Configuration()
        {
            InitializeComponent();
        }

        private void color_Click(object sender, EventArgs e)
        {
            ColorDialog ColorForm = new ColorDialog();
            if (ColorForm.ShowDialog() == DialogResult.OK)
            {
                Color GetColor = ColorForm.Color;
                //GetColor就是用户选择的颜色，接下来就可以使用该颜色了
                btnColor.BackColor = GetColor;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            Delete?.Invoke(this);
        }
    }
}
