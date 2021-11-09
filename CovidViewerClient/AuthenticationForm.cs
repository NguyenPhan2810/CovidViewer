using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsynchronousSocketHelper;

namespace CovidViewerClient
{
    public partial class AuthenticationForm : Form
    {
        private AsynchronousSocketClient socketClient = new AsynchronousSocketClient();

        public AuthenticationForm()
        {
            InitializeComponent();
        }

        private void AuthenticationForm_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = checkBoxShowPassword.CheckState == CheckState.Unchecked;
        }

        private void AuthenticationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkBoxRememberPassword.CheckState == CheckState.Checked)
            {
                Properties.Settings.Default.PasswordText = textBoxPassword.Text;
            }
            else
            {
                Properties.Settings.Default.PasswordText = "";
            }

            Properties.Settings.Default.Save();
        }

        private void buttonSignUp_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(socketClient.SendMessage("Hey there, im signing up", MessageHeader.SignUp));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error" + ex);
            }
        }

        private void buttonSignIn_Click(object sender, EventArgs e)
        {

        }

        static string MessageReceive1(string message)
        {
            return message.ToUpper();
        }
    }
    
}
