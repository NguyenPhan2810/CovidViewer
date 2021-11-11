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
using Newtonsoft.Json;
using System.Threading;


namespace CovidViewerClient
{
    public partial class AuthenticationForm : Form
    {
        MainForm mainForm = new MainForm();

        private AsynchronousSocketClient socketClient = null;

        Account account = new Account();

        Thread connectionThread;
        Mutex mutex = new Mutex();

        private string hostName;
        private int port;

        public AuthenticationForm()
        {
            InitializeComponent();            

        }

        private void ConnectionThread()
        {
            while (true)
            {
                mutex.WaitOne();

                bool isRunning = socketClient.IsRunning();
                

                if (buttonSignIn.InvokeRequired)
                    Invoke((Action)(delegate { UpdateButtonState(); }));
                if (buttonSignUp.InvokeRequired)
                    Invoke((Action)(delegate { UpdateButtonState(); }));
                if (mainForm.comboBoxCountry.InvokeRequired)
                    Invoke((Action)delegate
                    {
                        mainForm.comboBoxCountry.Enabled = isRunning;
                        if (isRunning)
                            mainForm.labelStatus.Text = $"Connected to {hostName}:{port}";
                        else
                            mainForm.labelStatus.Text = "Lost connection to the server!";
                    });



                Action setStatus = delegate
                {
                    if (isRunning)
                        labelStatus.Text = $"Connected to {hostName}:{port}";
                    else
                        labelStatus.Text = $"Connecting to {hostName}:{port}";
                };
                if (labelStatus.InvokeRequired)
                    labelStatus.Invoke(setStatus);



                if (!isRunning)
                {
                    while(!socketClient.Start())
                    {
                        Thread.Sleep(1000);
                    }
                }

                mutex.ReleaseMutex();

                Thread.Sleep(200);
            }
        }

        private void AuthenticationForm_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            account.Username = Properties.Settings.Default.UsernameText;
            account.Password = Properties.Settings.Default.PasswordText;

            hostName = Properties.Settings.Default.IpAddress;
            port = Properties.Settings.Default.Port;


            socketClient = new CovidViewerClientSocket(this, mainForm, hostName, port);

            connectionThread = new Thread(ConnectionThread);
            connectionThread.Start();
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            textBoxPassword.UseSystemPasswordChar = checkBoxShowPassword.CheckState == CheckState.Unchecked;
        }

        private void AuthenticationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (socketClient != null)
                socketClient.Stop();

            connectionThread.Abort();

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
            SignUp();
        }

        public void ShowMessageBox(string message)
        {
            MessageBox.Show(message);
        }

        private void SignUp()
        {
            account.Username = textBoxUsername.Text;
            account.Password = textBoxPassword.Text;
            
            socketClient.SendMessage(JsonConvert.SerializeObject(account), ClientHeader.SignUpRequest);
        }

        private void SignInRequeset()
        {
            account.Username = textBoxUsername.Text;
            account.Password = textBoxPassword.Text;
            socketClient.SendMessage(JsonConvert.SerializeObject(account), ClientHeader.SignInRequest);
        }

        Thread signInThread;

        private void SignInThread()
        {
            if (InvokeRequired)
            {
                Action hideAction = delegate
                {
                    SignInThread();
                };
                Invoke(hideAction);
                return;
            }

            Hide();
            mainForm.client = socketClient;
            mainForm.ShowDialog();

            Close();
        }

        public void SignIn()
        {
            if(signInThread != null)
            {
                signInThread.Abort();
                signInThread.Join();
            }

            signInThread = new Thread(SignInThread);
            signInThread.Start();
        }

        private void buttonSignIn_Click(object sender, EventArgs e)
        {
            SignInRequeset();
        }

        static string MessageReceive1(string message)
        {
            return message.ToUpper();
        }

        private void textBoxUsername_TextChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (textBoxUsername.Text != "" && textBoxPassword.Text != "" && socketClient.IsRunning())
            {
                buttonSignIn.Enabled = true;
                buttonSignUp.Enabled = true;
            }
            else
            {
                buttonSignIn.Enabled = false;
                buttonSignUp.Enabled = false;
            }
        }
    }

    public class Account
    {
        public string Username = null;
        public string Password = null;
    }
}
