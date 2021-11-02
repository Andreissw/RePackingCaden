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
    public partial class ConfirmUser : Form
    {
        public ConfirmUser()
        {
            InitializeComponent();
        }
        public int UserID { get; set; }
        public string UserName { get; set; }
        private void ConfirmUser_Load(object sender, EventArgs e)
        {
            textBox1.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!GetUser())
            {
                MessageBox.Show("Не верный логин");
                textBox1.Clear();
                textBox1.Select();
                return;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (!GetUser())
                {
                    MessageBox.Show("Не верный логин");
                    textBox1.Clear();
                    textBox1.Select();
                    return;
                }

                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        bool GetUser()
        {
            using (FASEntities FAS = new FASEntities())
            {
                
                var r = FAS.FAS_Users.Where(c => c.RFID == textBox1.Text && c.IsActiv == true).Select(c => c.UserID == c.UserID).FirstOrDefault();
                
                if (r == false)
                    return false; //Не верный логин

                UserID = FAS.FAS_Users.Where(c => c.RFID == textBox1.Text ).Select(c => c.UserID).FirstOrDefault();
                UserName = FAS.FAS_Users.Where(c => c.RFID == textBox1.Text).Select(c => c.UserName).FirstOrDefault();
                return true;// Верный

            }
        }
    }
}
