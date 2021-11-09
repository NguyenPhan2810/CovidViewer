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

namespace CovidViewerClient
{
    public partial class CovidViewerServerForm : Form
    {
        Thread listeningThread;

        CovidViewerServerSocket server = new CovidViewerServerSocket();

        public CovidViewerServerForm()
        {
            InitializeComponent();

            listeningThread = new Thread(server.StartListening);
        }

        private void CovidViewerServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (listeningThread.IsAlive)
            {
                server.StopListening();
                listeningThread.Join();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            listeningThread.Start();
        }

        private void buttonFetch_Click(object sender, EventArgs e)
        {
            Fetch();
        }

        private void Fetch()
        {

        }
    }

    class CovidViewerServerSocket : AsynchronousSocketServer
    {
        public override string MessageReceived(string message, MessageHeader header)
        {
            string echoMessage = "";
            switch (header)
            {
                case MessageHeader.SignUp:
                    echoMessage = "Hey I signed you up!";
                    break;
                case MessageHeader.SignIn:
                    break;
                case MessageHeader.Data:
                    break;
                default:
                    break;
            }

            return echoMessage;
        }
    }

}
