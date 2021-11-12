using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using AsynchronousSocketHelper;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace CovidViewerServer
{
    public partial class CovidViewerServerForm : Form
    {
        CovidViewerServerSocket server;

        System.Windows.Forms.Timer statusTimer = new System.Windows.Forms.Timer();
        Mutex mutex = new Mutex();

        public CovidViewerServerForm()
        {
            InitializeComponent();

        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (server.IsFetching)
            {
                textBoxStatus.Text = $"Fetching data...";
            }
            else if (server.ClientsConnected > 0)
            {
                textBoxStatus.Text = $"{server.ClientsConnected} clients connected.";
            }
            else if (server.IsListening)
            {
                textBoxStatus.Text = $"Listening for clients...";
            }
        }

        private void CovidViewerServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.StopListening();
            

            Properties.Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            server = new CovidViewerServerSocket(this,
                Properties.Settings.Default.Port);
            
            
            statusTimer.Interval = (int)(Properties.Settings.Default.StatusUpdateInterval * 1000);
            statusTimer.Tick += StatusTimer_Tick;
            statusTimer.Start();
           
        }

        private void buttonFetch_Click(object sender, EventArgs e)
        {
            server.Fetch();
        }

        private void CovidViewerServerForm_Shown(object sender, EventArgs e)
        {
            server.StartListening();

            new Thread(() =>
            {
                while (server.Ipv4 == "" || server.Ipv4 == null)
                {
                    Thread.Sleep(100);
                }

                textBoxIp.Invoke((Action)(() => textBoxIp.Text = $"Ipv4: {server.Ipv4}:{server.Port}"));
            }).Start();
        }
    }

    class CovidViewerServerSocket : AsynchronousSocketServer
    {
        protected CovidViewerServerForm serverForm = null;
        Dictionary<string, string> accounts = new Dictionary<string, string>();

        public string passwordPath = "passwords.json";

        public bool IsFetching { get; protected set; } = false;


        List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

        Mutex mutex = new Mutex();
        Thread fetchThread = null;
        Thread listeningThread = null;

        public CovidViewerServerSocket(CovidViewerServerForm theForm, int port)
            : base(port)
        {
            try
            {
                accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(passwordPath));
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        ManualResetEvent fetDataOnce = new ManualResetEvent(false);

        public override void StartListening()
        {
            if (fetchThread == null)
            {
                fetchThread = new Thread(FetchData);
                fetchThread.Start();
            }

        }

        public override void StopListening()
        {
            base.StopListening();

            if (fetchThread != null)
            {
                fetchThread.Abort();
                fetchThread = null;
            }

            File.WriteAllText(passwordPath, JsonConvert.SerializeObject(accounts, Formatting.Indented));
        }

        public void Fetch()
        {
            mutex.WaitOne();
            fetchThread.Interrupt();
            mutex.ReleaseMutex();
        }

        protected void FetchData()
        {
            while (true)
            {
                try
                {
                    mutex.WaitOne();
                    IsFetching = true;
                    mutex.ReleaseMutex();
                    string rawData = new WebClient().DownloadString("https://coronavirus-19-api.herokuapp.com/countries");

                    mutex.WaitOne();
                    data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(rawData);
                    if (!IsListening)
                    {
                        listeningThread = new Thread(base.StartListening);
                        listeningThread.Start();
                    }
                    IsFetching = false;
                    mutex.ReleaseMutex();

                    Thread.Sleep((int)(Properties.Settings.Default.FetchInterval * 1000));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        protected override void MessageReceived(string message, ClientHeader header, Socket handler)
        {
            try
            {
                switch (header)
                {
                    case ClientHeader.SignUpRequest:
                        {
                            Account account = JsonConvert.DeserializeObject<Account>(message);

                            if (!accounts.ContainsKey(account.Username))
                            {
                                accounts.Add(account.Username, account.Password);
                                Send(handler, message, ServerHeader.SignUpSuccess);
                            }
                            else
                            {
                                Send(handler, message, ServerHeader.SignUpFailed);
                            }
                            break;
                        }

                    case ClientHeader.SignInRequest:
                        {
                            Account account = JsonConvert.DeserializeObject<Account>(message);

                            if (!accounts.ContainsKey(account.Username))
                            {
                                Send(handler, message, ServerHeader.SignInFailed);
                            }
                            else if (accounts[account.Username] == account.Password)
                            {
                                Send(handler, message, ServerHeader.SignInSuccess);
                            }
                            else
                            {
                                Send(handler, message, ServerHeader.SignInFailed);
                            }
                            break;
                        }

                    case ClientHeader.RequestCountryHeader:
                        {
                            string headerMessage = "";

                            mutex.WaitOne();
                            string[] headers = new string[data.Count];
                            for (int i = 0; i < data.Count; ++i)
                            {
                                headers[i] = data[i]["country"];
                            }
                            headerMessage = JsonConvert.SerializeObject(headers);
                            mutex.ReleaseMutex();

                            Send(handler, headerMessage, ServerHeader.FetchCountryHeader);
                            break;
                        }

                    case ClientHeader.RequestCountryData:
                        {
                            if (message == "")
                                break;

                            string country = message;

                            mutex.WaitOne();
                            
                            var foundCountry = Array.Find(data.ToArray(), delegate (Dictionary<string, string> val)
                            {
                                return val["country"] == country;
                            });
                            
                            mutex.ReleaseMutex();

                            Send(handler,
                                JsonConvert.SerializeObject(foundCountry),
                                ServerHeader.FetchCountryData);
                            break;
                        }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }
    }


    public class Account
    {
        public string Username = null;
        public string Password = null;
    }
}
