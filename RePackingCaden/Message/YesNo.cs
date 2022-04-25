using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RePackingCaden.Message
{
    public partial class YesNo : Form
    {
        public YesNo(string text)
        {
            InitializeComponent();
            label1.Text = text;
        }

        private void YesNo_Load(object sender, EventArgs e)
        {

        }

        private void YesNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
