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

    public class Account
    {
        public string Username = null;
        public string Password = null;
    }
}
