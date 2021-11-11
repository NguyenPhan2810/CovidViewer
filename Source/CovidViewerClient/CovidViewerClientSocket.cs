using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsynchronousSocketHelper;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;

namespace CovidViewerClient
{

    class CovidViewerClientSocket : AsynchronousSocketClient
    {
        protected AuthenticationForm authenForm = null;
        protected MainForm mainForm = null;
        public CovidViewerClientSocket(AuthenticationForm authenForm, MainForm mainForm, string hostName, int port)
            : base(hostName, port)
        {
            this.authenForm = authenForm;
            this.mainForm = mainForm;
        }

        protected override void MessageReceived(string message, ServerHeader header, Socket handler)
        {
            Debug.WriteLine($"Debug 2: {message}");
            switch (header)
            {
                case ServerHeader.SignUpSuccess:
                    authenForm.ShowMessageBox("Sign up successfully!");
                    break;
                case ServerHeader.SignUpFailed:
                    try
                    {
                        Account acc = JsonConvert.DeserializeObject<Account>(message);
                        authenForm.ShowMessageBox($"Username '{acc.Username}' already existed");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Error in signing up\n" + ex.ToString());
                    }
                    break;
                case ServerHeader.SignInSuccess:
                    authenForm.SignIn();
                    break;

                case ServerHeader.SignInFailed:
                    MessageBox.Show("Authentication failed!");
                    break;
                case ServerHeader.FetchCountryHeader:
                    mainForm.FetchHeader(message);
                    break;
                case ServerHeader.FetchCountryData:
                    mainForm.FetchData(message);
                    break;
                default:
                    break;
            }
        }
    }
}
